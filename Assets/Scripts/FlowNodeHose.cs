using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class FlowNodeHose : FlowNode
{
    [Header("Spline Settings")]
    [Tooltip("Four control points that define the cubic Bezier spline for the hose path.")]
    public Transform[] controlPoints;

    [Header("Fluid Settings")]
    [Tooltip("Prefab used for fluid particles (e.g., small spheres).")]
    public GameObject particlePrefab;
   
    [Tooltip("Multiplier for particle flow along the hose spline.")]
    public float maxFlowRate = 6f;
    [Tooltip("Normalized flow rate (0-1) controlling the speed and visibility of particles.")]
    public float normalizedFlowRate = 0f;
    [Tooltip("Number of particles flowing through the hose.")]
    public int particleCount = 3;

    [Tooltip("If true, particles flow backwards along the spline.")]
    public bool isReversed = false;
    [Tooltip("Duration (in seconds) for particles to fade in/out when flow stopped.")]
    public float fadeDuration = 0.5f;
    [Tooltip("Whether flow is clamped")]
    public bool isClamped = true;

    [Header("Hose Rendering")]
    [Tooltip("Material used to render the hose line.")]
    public Material hoseMaterial;
    [Tooltip("Diameter (thickness) of the hose line.")]
    public float hoseDiameter = 0.1f;
    [Tooltip("Number of segments used to render the hose line (higher = smoother curve).")]
    public int hoseSegments = 20;
    
    private List<GameObject> particles = new List<GameObject>();
    private List<Renderer> particleRenderers = new List<Renderer>();
    private float[] tValues;
    private LineRenderer lineRenderer;

    // Fade state
    private float fadeTimer = 0f;
    private bool fadingOut = true;
    private bool fadingIn = false;
    private bool transparencySupported = false; // per particle material ability
    private List<MaterialPropertyBlock> mpbList = new List<MaterialPropertyBlock>();
    private Color fluidColor = Color.cyan; // default, otherwise taken from particlePrefab

    private List<GameObject> clamps = new List<GameObject>();  // to track attached clamps

    void Awake()
    {
        base.Awake();
        // Setup LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        if (lineRenderer == null)
            lineRenderer = gameObject.AddComponent<LineRenderer>();

        lineRenderer.material = hoseMaterial;
        lineRenderer.startWidth = hoseDiameter;
        lineRenderer.endWidth = hoseDiameter;
        lineRenderer.positionCount = hoseSegments + 1;

    }

    void Start()
    {
        // Get the color from the particlePrefab's material (if available)
        fluidColor = Color.cyan; // default fallback
        if (particlePrefab != null)
        {
            var renderer = particlePrefab.GetComponent<Renderer>();
            if (renderer != null && renderer.sharedMaterial != null && renderer.sharedMaterial.HasProperty("_Color"))
            {
            fluidColor = renderer.sharedMaterial.GetColor("_Color");
            }
        }
        if (!Application.isPlaying) return;

        tValues = new float[particleCount];

        particles.Clear();
        particleRenderers.Clear();
        mpbList.Clear();

        // Instantiate particles
        for (int i = 0; i < particleCount; i++)
        {
            GameObject p = Instantiate(particlePrefab, transform);
            particles.Add(p);

            var r = p.GetComponent<Renderer>();
            if (r == null)
            {
                r = p.AddComponent<MeshRenderer>(); // fallback, but ideally prefab has a Renderer
            }

            particleRenderers.Add(r);

            // Prepare per-particle MPB
            var mpb = new MaterialPropertyBlock();
            mpb.SetColor("_Color", fluidColor);
            r.SetPropertyBlock(mpb);
            mpbList.Add(mpb);

            tValues[i] = i / (float)particleCount;
        }

        // Start with particles faded out if no flow
        if (normalizedFlowRate == 0f || particleCount == 0)
        {
            SetParticleAlpha(0f);
        }
        if (normalizedFlowRate > 0f && particleCount > 0){
            SetFlow(normalizedFlowRate);
        }
        

        // hide particlePrefab (only show instantiated particles)
        if (particlePrefab != null)
            particlePrefab.SetActive(false);

        UpdateHoseLineRenderer();

    }

    void Update()
    {
        // Only update line renderer in edit mode
        if (!Application.isPlaying)
        {
            UpdateHoseLineRenderer();
        }   

        if (!Application.isPlaying) return;

        HandleFade(); 

        if (normalizedFlowRate == 0f || particleCount == 0) return;

        // Animate particles 
        for (int i = 0; i < particles.Count; i++)
        {
            float direction = isReversed ? -1f : 1f;
            tValues[i] += Time.deltaTime * normalizedFlowRate * maxFlowRate * 0.1f * direction;

            if (tValues[i] > 1f) tValues[i] = 0f;
            if (tValues[i] < 0f) tValues[i] = 1f;

            Vector3 pos = GetBezierPoint(tValues[i],
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position);

            particles[i].transform.position = pos;
        }
    }

    Vector3 GetBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        return u * u * u * p0 +
               3 * u * u * t * p1 +
               3 * u * t * t * p2 +
               t * t * t * p3;
    }

    private void HandleFade()
    {
        //Debug.Log($"Fading Out : {fadingOut}");
        if (fadingOut)
        {
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, Mathf.Clamp01(fadeTimer / fadeDuration));
            
            SetParticleAlpha(alpha);

            if (fadeTimer >= fadeDuration)
            {
                fadingOut = false;
            }
        }
        else if (fadingIn)
        {
            
            fadeTimer += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, Mathf.Clamp01(fadeTimer / fadeDuration));
            SetParticleAlpha(alpha);

            if (fadeTimer >= fadeDuration)
            {
                fadingIn = false;
            }
        }
    }

    private void SetParticleAlpha(float alpha)
    {
        for (int i = 0; i < particleRenderers.Count; i++)
        {
            var r = particleRenderers[i];
            if (r == null) continue;
            r.material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

            var mpb = new MaterialPropertyBlock();
            r.GetPropertyBlock(mpb); // pull current state
            Color c = fluidColor;
            c.a = alpha;
            mpb.SetColor("_Color", c);
            r.SetPropertyBlock(mpb);
        }
        //Debug.Log($"Set particle alpha to {alpha}");
    }

    // important -- MOMORY LEAK AVOIDANCE, only use in edit mode or on changes
    private void UpdateMeshCollider()
    {
        MeshCollider meshCollider = GetComponent<MeshCollider>();
        // check if meshCollider exists, if not add one
        if (meshCollider == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
            
        }else {
            meshCollider.sharedMesh = null; // clear existing
        }
        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, true);
        meshCollider.sharedMesh = mesh;
    }

    private float RaySegmentDistanceSqr(
        Ray ray,
        Vector3 a,
        Vector3 b,
        out float rayT)
    {
        Vector3 p = ray.origin;
        Vector3 r = ray.direction;

        Vector3 q = a;
        Vector3 s = b - a;

        float rDotR = Vector3.Dot(r, r);
        float sDotS = Vector3.Dot(s, s);
        float rDotS = Vector3.Dot(r, s);
        Vector3 qMinusP = q - p;
        float rDotQMinusP = Vector3.Dot(r, qMinusP);
        float sDotQMinusP = Vector3.Dot(s, qMinusP);

        float denom = rDotR * sDotS - rDotS * rDotS;

        float t, u;

        // Handle near-parallel case
        if (Mathf.Abs(denom) < 1e-6f)
        {
            t = rDotQMinusP / rDotR;
            u = Mathf.Clamp01(rDotQMinusP / (rDotS + 1e-6f));
        }
        else
        {
            t = (rDotQMinusP * sDotS - sDotQMinusP * rDotS) / denom;
            u = (rDotQMinusP * rDotS - sDotQMinusP * rDotR) / denom;
            u = Mathf.Clamp01(u);
        }

        rayT = t;

        Vector3 closestOnRay = p + t * r;
        Vector3 closestOnSeg = q + u * s;

        return (closestOnRay - closestOnSeg).sqrMagnitude;
    }

    // Draw spline in editor for preview
    void OnDrawGizmos()
    {
        if (controlPoints == null || controlPoints.Length < 4) return;

        Gizmos.color = Color.yellow;
        Vector3 prevPos = controlPoints[0].position;

        for (int i = 1; i <= hoseSegments; i++)
        {
            float t = i / (float)hoseSegments;
            Vector3 pos = GetBezierPoint(t,
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position);

            Gizmos.DrawLine(prevPos, pos);
            prevPos = pos;
        }
    }

    // Public methods for flow controls
    // flow: normalized 0-1
    public override void SetFlow(float newFlow)
    {
        // round to whole number for stability
        newFlow = isClamped? 0f : Mathf.Round(newFlow * 100f) / 100f; 
    
        fadeTimer = 0f;

        if (newFlow <= 0.01f && normalizedFlowRate > 0f )
        {
            fadingOut = true;
            fadingIn = false;     
            //Debug.Log("FlowNodeHose SetFlow fading out");   
        }
        else if (newFlow > 0.01f && normalizedFlowRate == 0f)
        {
            fadingIn = true;
            fadingOut = false;
        }else {
            fadingIn = false;
            fadingOut = false;
        }
        
        // update flow rate
        normalizedFlowRate = newFlow;
        
        // propogate to downstream hoses if any
        //Debug.Log("FlowNodeHose SetFlow called. Flow: " + flow);
        if (downstreamFlowNodes != null)
        {
            foreach (var hose in downstreamFlowNodes)
            {
                hose?.SetFlow(newFlow);
            }
        }
    }

    public void ToggleReverse() => isReversed = !isReversed;
    public void SetReverse(bool reverse) => isReversed = reverse;

    public void SetClamp(GameObject clamp, bool clampState)
    {
        if (clampState)
        {
            if (!clamps.Contains(clamp))
            {
                clamps.Add(clamp);
            }
        }
        else
        {
            if (clamps.Contains(clamp))
            {
                clamps.Remove(clamp);
            }
        }

        isClamped = clamps.Count > 0;
        
        base.UpdateFlowSystem();
    }

    public void UpdateHoseLineRenderer()
    {
        if (controlPoints == null || controlPoints.Length < 4) return;

        for (int i = 0; i <= hoseSegments; i++)
        {
            float t = i / (float)hoseSegments;
            Vector3 pos = GetBezierPoint(t,
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position);

            lineRenderer.SetPosition(i, pos);
        }

        UpdateMeshCollider();
    }

    // for collistion detection without need for line collider
    public bool RaycastHose(Ray ray, out float rayT, out Vector3 hitPoint)
    {
        float hoseRadius = hoseDiameter * 0.5f;
        rayT = float.PositiveInfinity;
        hitPoint = Vector3.zero;

        LineRenderer lr = GetComponent<LineRenderer>();
        if (lr == null || lr.positionCount < 2)
            return false;

        float radiusSqr = hoseRadius * hoseRadius;
        bool hit = false;

        for (int i = 0; i < lr.positionCount - 1; i++)
        {
            Vector3 a = lr.GetPosition(i);
            Vector3 b = lr.GetPosition(i + 1);

            float t;
            float distSqr = RaySegmentDistanceSqr(ray, a, b, out t);

            if (distSqr <= radiusSqr && t >= 0f)
            {
                if (t < rayT)
                {
                    rayT = t;
                    hitPoint = ray.origin + ray.direction * t;
                    hit = true;
                }
            }
        }

        return hit;
    }

}

// Utility class to find closest FlowNodeHose from a ray
public static class HoseRaycastManager
{
    public static FlowNodeHose GetClosestHose(Ray ray)
    {
        FlowNodeHose best = null;
        float bestT = float.PositiveInfinity;

        foreach (var hose in Object.FindObjectsOfType<FlowNodeHose>())
        {
            float t;
            Vector3 hitPoint;

            if (hose.RaycastHose(ray, out t, out hitPoint))
            {
                if (t < bestT)
                {
                    bestT = t;
                    best = hose;
                }
            }
        }

        return best;
    }
}
