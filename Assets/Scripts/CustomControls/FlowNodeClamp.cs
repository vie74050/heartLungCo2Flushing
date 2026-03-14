using UnityEngine;
using UnityEngine.EventSystems;

// Makes a draggable g.o that can interact with FlowNodeHose LineRenderer component. 
// - raycasting against the LineRenderer to find the closest point on the hose to drop position.
// The clamp object is then positioned and oriented at that point.

public class FlowNodeClamp : MonoBehaviour
{
    //public Color highlightColor = Color.yellow;
    private FlowNodeHose controlledHose;
    private Transform initTn;

    private bool isDragging { get; set; } = false;

    [System.Serializable]
    public struct HoseAttachment // store parameters for clamp on hose
    {
        public FlowNodeHose hose;
        public int segmentIndex;
        public float t;
    }
    private HoseAttachment? attachment;

    private Interactible interactible;
    void Start()
    {
        // for reference to reset position if needed
        initTn = new GameObject("InitTn").transform;
        initTn.SetPositionAndRotation(transform.position, transform.rotation);
        interactible = GetComponent<Interactible>();
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
    private void FindHose()
    {
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

    // Only allow one clamp dragged at a time by checking all FlowNodeClamps in scene for isDragging
    private FlowNodeClamp GetIsDraggingClamp() 
    {
        // if any clamp in scene isDragging, return that clamp
        foreach (var c in Object.FindObjectsOfType<FlowNodeClamp>())        {
            if (c.isDragging)
            {
                return c;
            }
        }
        return null;
    }
    // when clicked, "pickup" clamp, hide it's collider and turn on find hose
    // isDragging until released attachment or dropped on dropzone
    private void OnMouseDown()
    {
        var draggingClamp = GetIsDraggingClamp();
        if (draggingClamp != null && draggingClamp != this)
        {
            return; // another clamp is already being dragged
        }

        SetDragging(true);
    }
    private void OnDrag()
    {
        FindHose();
        // set the mouse cursor interactible cursor when dragging
        /*if (interactible != null && interactible.cursorTexture != null)
        {
            Cursor.SetCursor(interactible.cursorTexture, Vector2.zero, CursorMode.Auto);
        }*/

        if (!attachment.HasValue)
        {   
            // follow mouse position in world space (keep original z)
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.WorldToScreenPoint(transform.position).z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            transform.position = worldPos;
        }
    }
    // detect mouse move while isDragging, find hose and attach to it
    private void Update()
    {
        if (isDragging)
        {
            OnDrag();
        }

        // if not attached, reset position to initial position
        if (Input.GetMouseButtonUp(0))
        {
            // Ignore "clamp" layer in raycast
            int clampLayer = LayerMask.NameToLayer("clamp");
            int layerMask = ~(1 << clampLayer);

            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, layerMask))
            {
                // check if over a hose
                if (attachment.HasValue && hit.collider.gameObject.GetComponent<FlowNodeHose>() == attachment.Value.hose)
                {
                    // stop dragging, release on hose
                    SetDragging(false);
                }else
                {
                    // check if over a dropzone
                    if (hit.collider.gameObject.GetComponent<Dropzone>() != null)
                    {
                        // if the dropzone checklist is empty, return the isDragging clamp to initial position
                        Dropzone dz = hit.collider.gameObject.GetComponent<Dropzone>();
                        if (dz.checklist.Count == 0)
                        {
                            
                            FlowNodeClamp draggingClamp = GetIsDraggingClamp();
                            if (draggingClamp != null)
                            {
                                draggingClamp.ResetPosition();
                            }
                            SetDragging(false);
                        }
                    }
                }
                
            }
        }               
    }
    public void ResetPosition()
    {
        transform.SetPositionAndRotation(initTn.position, initTn.rotation);
        if (attachment.HasValue)
        {
            attachment.Value.hose.SetClamp(gameObject, false);
            attachment = null;
        }
    }
    public void FollowHose()
    {
        if (!attachment.HasValue) return;
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

    public void SetDragging(bool dragging)
    {
        isDragging = dragging;
        GetComponent<Collider>().enabled = !dragging; // re-enable collider when not dragging
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



