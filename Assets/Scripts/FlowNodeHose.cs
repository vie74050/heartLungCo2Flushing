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
    private MeshCollider meshCollider; // for hose collider if needed

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

    }

    void Update()
    {
        // Always update hose line (editor + play mode)
        UpdateHoseLineRenderer();

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

    private void UpdateHoseLineRenderer()
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

    Vector3 GetBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        return u * u * u * p0 +
               3 * u * u * t * p1 +
               3 * u * t * t * p2 +
               t * t * t * p3;
    }

    private void UpdateMeshCollider()
    {
        meshCollider = GetComponent<MeshCollider>();
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
            Debug.Log("FlowNodeHose SetFlow fading out");   
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


}