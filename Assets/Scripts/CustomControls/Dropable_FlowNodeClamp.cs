using UnityEngine;
using UnityEngine.EventSystems;

// Makes a draggable g.o that can interact with FlowNodeHose LineRenderer component. 
// - raycasting against the LineRenderer to find the closest point on the hose to drop position.
// The clamp object is then positioned and oriented at that point.

public class SO_FlowNodeClamp : SO_Dropable
{
    //public Color highlightColor = Color.yellow;
    private FlowNodeHose controlledHose;
    private Transform initTn;

    [System.Serializable]
    public struct HoseAttachment // store parameters for clamp on hose
    {
        public FlowNodeHose hose;
        public int segmentIndex;
        public float t;
    }
    private HoseAttachment? attachment;

    public override void Init()
    {
        // for reference to reset position if needed
        initTn = transform;
        base.Init();
    }

    void Update()
    {
        if (attachment.HasValue)
        {
            FollowHose();
        }
    }
    private void FollowHose()
    {
        var a = attachment.Value;

        LineRenderer lr = a.hose.GetComponent<LineRenderer>();
        if (lr == null) return;

        Vector3 p1 = lr.GetPosition(a.segmentIndex);
        Vector3 p2 = lr.GetPosition(a.segmentIndex + 1);

        Vector3 cp = Vector3.Lerp(p1, p2, a.t);

        // Compute orientation again
        Vector3 dir = (p2 - p1).normalized;
        Vector3 normal = Vector3.Cross(-dir, Camera.main.transform.forward).normalized;
        Quaternion rot = Quaternion.LookRotation(dir, normal);

        transform.SetPositionAndRotation(cp, rot);
    }
    private bool TryGetClampPoseOnHose(
        FlowNodeHose hose,
        Vector3 worldPoint,
        out Vector3 closestPoint,
        out Quaternion rotation,
        out int closestSegment,
        out float closestT)
    {
        closestPoint = Vector3.zero;
        rotation = Quaternion.identity;
        closestSegment = 0;
        closestT = 0f;

        LineRenderer lr = hose.GetComponent<LineRenderer>();
        if (lr == null || lr.positionCount < 2)
            return false;

        float minDist = float.MaxValue;

        for (int i = 0; i < lr.positionCount - 1; i++)
        {
            Vector3 p1 = lr.GetPosition(i);
            Vector3 p2 = lr.GetPosition(i + 1);

            Vector3 seg = p2 - p1;
            float t = Mathf.Clamp01(Vector3.Dot(worldPoint - p1, seg) / seg.sqrMagnitude);
            Vector3 pointOnSegment = p1 + seg * t;

            float dist = Vector3.SqrMagnitude(worldPoint - pointOnSegment);
            if (dist < minDist)
            {
                minDist = dist;
                closestPoint = pointOnSegment;
                closestSegment = i;
                closestT = t;
            }
        }

        Vector3 dir = (lr.GetPosition(closestSegment + 1) - lr.GetPosition(closestSegment)).normalized;
        Vector3 normal = Vector3.Cross(-dir, Camera.main.transform.forward).normalized;

        rotation = Quaternion.LookRotation(dir, normal);
        return true;
    }

    protected override void OnDrag()
    {
        //Debug.Log("Dragging FlowNodeClamp");

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (HoseRaycastManager.GetClosestHose(ray, out var hose, out var hoseHitPoint))
        {
            if (hose != controlledHose && controlledHose != null)
            {
                // release the previous hose
                controlledHose.SetClamp(gameObject, false);
            }
            controlledHose = hose;

            // attach to hose at closest point
            if (TryGetClampPoseOnHose(controlledHose, hoseHitPoint, out var cp, out var rot,
                        out int segIndex, out float t))
            {
                attachment = new HoseAttachment
                {
                    hose = controlledHose,
                    segmentIndex = segIndex,
                    t = t
                };

                transform.SetPositionAndRotation(cp, rot);
                controlledHose.SetClamp(gameObject, true);
            }

        }
        else
        {
            attachment = null;
            if (controlledHose != null)
            {
                controlledHose.SetClamp(gameObject, false);
                controlledHose = null;
            }
        }
    }

}

// Utility class to find closest FlowNodeHose from a ray
public static class HoseRaycastManager
{
    public static bool GetClosestHose(Ray ray, out FlowNodeHose hose, out Vector3 hitPoint)
    {
        hose = null;
        hitPoint = Vector3.zero;

        float bestT = float.PositiveInfinity;

        foreach (var h in Object.FindObjectsOfType<FlowNodeHose>())
        {
            float t;
            Vector3 hp;

            if (h.RaycastHose(ray, out t, out hp))
            {
                if (t < bestT)
                {
                    bestT = t;
                    hose = h;
                    hitPoint = hp;
                }
            }
        }

        return hose != null;
    }

}



