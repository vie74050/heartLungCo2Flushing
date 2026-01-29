using UnityEngine;
using UnityEngine.EventSystems;

// Component will add functionality to control clamped stated of the FlowNodeHose if attached

public class SO_FlowNodeClamp : SO_Dropable
{
    private FlowNodeHose controlledHose;
    private Transform initTn;
    private Vector3 droppedPos;
    private Quaternion droppedRot;
    private Dropzone dz;
    
    protected override void OnDrag()
    {
        base.OnDrag();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dropzoneMask))
        {
            Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
            // get dropzone being hovered over
            dz = GetOverDropzone();
            // if over dropzone with FlowNodeHose, update clamp state
            if (dz != null)
            {
                controlledHose = dz.GetComponent<FlowNodeHose>();
                                
                
                // Orient the clamp normal to the hose (LineRenderer) at the closest point
                LineRenderer lr = controlledHose?.GetComponent<LineRenderer>();
                if (lr != null)
                {
                    // Find closest point on the line to the hit point
                    float minDist = float.MaxValue;
                    Vector3 closestPoint = lr.GetPosition(0);
                    int closestSegment = 0;
                    for (int i = 0; i < lr.positionCount - 1; i++)
                    {
                        Vector3 p1 = lr.GetPosition(i);
                        Vector3 p2 = lr.GetPosition(i + 1);
                        Vector3 proj = Vector3.Project(hit.point - p1, p2 - p1);
                        float t = Mathf.Clamp01(Vector3.Dot(proj, p2 - p1) / (p2 - p1).sqrMagnitude);
                        Vector3 pointOnSegment = Vector3.Lerp(p1, p2, t);
                        float dist = Vector3.Distance(hit.point, pointOnSegment);
                        if (dist < minDist)
                        {
                            minDist = dist;
                            closestPoint = pointOnSegment;
                            closestSegment = i;
                        }
                    }
                    // Find direction of the segment
                    Vector3 dir = (lr.GetPosition(closestSegment + 1) - lr.GetPosition(closestSegment)).normalized;
                    // Find a normal vector (arbitrary, but perpendicular)
                    Vector3 normal = Vector3.Cross(-dir, Camera.main.transform.forward).normalized;
                    // Set rotation so clamp's up is normal to hose
                    transform.rotation = Quaternion.LookRotation(dir, normal);
                }

                // store dropped position to snap to on drop
                droppedPos = hit.point;
                // store dropped rotation to snap to on drop
                droppedRot = transform.rotation;
            }
        }
        else
        {
            controlledHose?.SetClamp(gameObject, false);
            controlledHose = null;
        }
    }

    // extend OnMouseUp to detect collision with FlowNodeHose and toggle clamp state
    protected override void OnDrop()
    {
       
        if (controlledHose != null)
        {
            controlledHose.SetClamp(gameObject, true);
            transform.position = droppedPos;
            transform.rotation = droppedRot;
            Debug.Log("SO_FlowNodeClamp dropped on FlowNodeHose - clamp ON");
        }

        if(dz!=null)
        {
            dz.checklist[0].isComplete = false;
        }
        
    }
}
 