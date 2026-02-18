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
    private List<MaterialPropertyBlock> mpbList = new List<MaterialPropertyBlock>();
    private Color fluidColor = Color.cyan; // default, otherwise taken from particlePrefab

    private List<GameObject> clamps = new List<GameObject>();  // to track attached clamps

    protected override void Awake()
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

    private System.Collections.IEnumerator HandleFade(bool fadeIn)
    {
        float startAlpha = fadeIn ? 0f : 1f;
        float endAlpha = fadeIn ? 1f : 0f;
        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            SetParticleAlpha(alpha);
            yield return null;
        }
        SetParticleAlpha(endAlpha);

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
        
        // Create a 3D tube mesh along the spline instead of baking the 2D line renderer mesh
        Mesh mesh = new Mesh();

        int radialSegments = 8; // Number of sides for the tube
        int lengthSegments = hoseSegments;
        float radius = hoseDiameter * 0.5f;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();

        // Generate vertices and normals
        for (int i = 0; i <= lengthSegments; i++)
        {
            float t = i / (float)lengthSegments;
            Vector3 center = GetBezierPoint(t,
            controlPoints[0].position,
            controlPoints[1].position,
            controlPoints[2].position,
            controlPoints[3].position);

            // Calculate tangent for orientation
            Vector3 tangent;
            if (i < lengthSegments)
            {
            float tNext = (i + 1) / (float)lengthSegments;
            tangent = (GetBezierPoint(tNext,
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position) - center).normalized;
            }
            else
            {
            float tPrev = (i - 1) / (float)lengthSegments;
            tangent = (center - GetBezierPoint(tPrev,
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position)).normalized;
            }

            // Find a vector not parallel to tangent for normal calculation
            Vector3 normal = Vector3.Cross(tangent, Vector3.up);
            if (normal.sqrMagnitude < 0.001f)
            normal = Vector3.Cross(tangent, Vector3.right);
            normal.Normalize();

            for (int j = 0; j < radialSegments; j++)
            {
            float angle = 2 * Mathf.PI * j / radialSegments;
            Quaternion rot = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, tangent);
            Vector3 dir = rot * normal;
            vertices.Add(center + dir * radius);
            normals.Add(dir);
            }
        }

        // Generate triangles
        for (int i = 0; i < lengthSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
            int current = i * radialSegments + j;
            int next = current + radialSegments;
            int nextJ = (j + 1) % radialSegments;

            int currentNextJ = i * radialSegments + nextJ;
            int nextNextJ = currentNextJ + radialSegments;

            // Two triangles per quad
            triangles.Add(current);
            triangles.Add(next);
            triangles.Add(nextNextJ);

            triangles.Add(current);
            triangles.Add(nextNextJ);
            triangles.Add(currentNextJ);
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetTriangles(triangles, 0);
        // Make collider radius 1.2x wider for better collision
        float colliderRadius = Mathf.Max(radius * 1.2f, 1f);

        // Generate collider mesh with wider radius
        List<Vector3> colliderVertices = new List<Vector3>();
        List<int> colliderTriangles = new List<int>();
        List<Vector3> colliderNormals = new List<Vector3>();

        for (int i = 0; i <= lengthSegments; i++)
        {
            float t = i / (float)lengthSegments;
            Vector3 center = GetBezierPoint(t,
            controlPoints[0].position,
            controlPoints[1].position,
            controlPoints[2].position,
            controlPoints[3].position);

            Vector3 tangent;
            if (i < lengthSegments)
            {
            float tNext = (i + 1) / (float)lengthSegments;
            tangent = (GetBezierPoint(tNext,
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position) - center).normalized;
            }
            else
            {
            float tPrev = (i - 1) / (float)lengthSegments;
            tangent = (center - GetBezierPoint(tPrev,
                controlPoints[0].position,
                controlPoints[1].position,
                controlPoints[2].position,
                controlPoints[3].position)).normalized;
            }

            Vector3 normal = Vector3.Cross(tangent, Vector3.up);
            if (normal.sqrMagnitude < 0.001f)
            normal = Vector3.Cross(tangent, Vector3.right);
            normal.Normalize();

            for (int j = 0; j < radialSegments; j++)
            {
            float angle = 2 * Mathf.PI * j / radialSegments;
            Quaternion rot = Quaternion.AngleAxis(Mathf.Rad2Deg * angle, tangent);
            Vector3 dir = rot * normal;
            colliderVertices.Add(center + dir * colliderRadius);
            colliderNormals.Add(dir);
            }
        }

        for (int i = 0; i < lengthSegments; i++)
        {
            for (int j = 0; j < radialSegments; j++)
            {
            int current = i * radialSegments + j;
            int next = current + radialSegments;
            int nextJ = (j + 1) % radialSegments;

            int currentNextJ = i * radialSegments + nextJ;
            int nextNextJ = currentNextJ + radialSegments;

            colliderTriangles.Add(current);
            colliderTriangles.Add(next);
            colliderTriangles.Add(nextNextJ);

            colliderTriangles.Add(current);
            colliderTriangles.Add(nextNextJ);
            colliderTriangles.Add(currentNextJ);
            }
        }

        Mesh colliderMesh = new Mesh();
        colliderMesh.SetVertices(colliderVertices);
        colliderMesh.SetNormals(colliderNormals);
        colliderMesh.SetTriangles(colliderTriangles, 0);

        meshCollider.sharedMesh = colliderMesh;

        // update mesh filter
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.sharedMesh = mesh;
               

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

    private void OnMouseOver() {
        SetHoseColor(new Color(1f, 1f, 0f, 0.5f));    
    }
  
    private void OnMouseExit()
    {
        SetHoseColor(new Color(1f, 1f, 1f, 0f));
    }
    // Public methods for flow controls
    // flow: normalized 0-1
    public override void SetFlow(float newFlow)
    {
        // round to whole number for stability
        newFlow = isClamped? 0f : Mathf.Round(newFlow * 100f) / 100f; 

        if (newFlow <= 0.0f && normalizedFlowRate > 0f )
        {
            // start fade out
            StartCoroutine(HandleFade(false));       
        }
        else if (newFlow > 0.0f && normalizedFlowRate == 0f)
        {
            // start fade in
            StartCoroutine(HandleFade(true));       
        }else {
           /// Debug.Log("FlowNodeHose SetFlow called. Flow: " + newFlow);
        }
        
        // update flow rate
        normalizedFlowRate = newFlow;
        
        // propogate to downstream hoses if any
        //Debug.Log("FlowNodeHose SetFlow called. Flow: " + flow);
        if (downstreamFlowNodes != null)
        {
            foreach (var hose in downstreamFlowNodes)
            {
                // error check that hose's downstreamFlowNodes is not empty and does not include this hose to avoid infinite loop, if it does, skip propogation and log error
                if (hose?.downstreamFlowNodes != null )
                {
                    if(hose.downstreamFlowNodes.Length > 0 && System.Array.Exists(hose.downstreamFlowNodes, n => n == this))
                    {
                        Debug.LogError("FlowNodeHose SetFlow error: downstream hose " + hose.name + " has this hose as downstream, skipping propogation to avoid infinite loop.");
                        continue;
                    }
                }
                hose?.SetFlow(newFlow);
            }
        }
    }

    public void ToggleReverse() => isReversed = !isReversed;
    public void SetReverse(bool reverse) => isReversed = reverse;

    // used by user-added clamps to dynamically clamp/unclamp the hose
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

        lineRenderer.positionCount = hoseSegments + 1;
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

        // if there are clamps attached, get SO_FlowNodeClamp component to call FollowHose
        foreach (var c in clamps)
        {
            var soClamp = c.GetComponent<SO_FlowNodeClamp>();
            soClamp?.FollowHose();
        }
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

    // public method to change color of hose
    public void SetHoseColor(Color newColor)
    {
        if (lineRenderer != null && lineRenderer.material != null)
        {
            lineRenderer.material.color = newColor;
        }
    }
}