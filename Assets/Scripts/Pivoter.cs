using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Pivoter.cs
 * 
 * Attach to an object you want to click on to pivot other objects around their pivots. e.g. scissors, doors, etc.
 * 
 * */

public class Pivoter : MonoBehaviour
{
    public bool isOpen = false;

    [Tooltip("The objects to pivot around their pivots")]
    public PivotItem[] pvObjs;

    private float mouseDowndT = 0;
    private int mouseDowndT_limit = 30;

    private void Start()
    {
        if (isOpen){
            foreach (PivotItem pi in pvObjs)
            {
                if (isOpen) pi.PivotAround(pi.max);
            }
        }
    }
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouseDowndT++;
        }
    }
    private void OnMouseDown()
    {
        if (mouseDowndT < mouseDowndT_limit)
        {
            ToggleOpen();
        }
        mouseDowndT = 0;
    }

    private void ToggleOpen()
    {
       foreach (PivotItem pi in pvObjs)
        {
            if (isOpen) pi.PivotAround(-pi.max);
            else pi.PivotAround(pi.max);
        }

        isOpen = !isOpen;
    }

}

[System.Serializable]
public class PivotItem
{
    public float max = 30;
    public Transform t;
    public Transform pivot; // optional
    public Vector3 rotaxis = Vector3.forward;
       
    public void PivotAround(float amt)
    {
        Vector3 pivotPt = (pivot != null)? pivot.position : t.position;
        Vector3 axis;
        
        if (pivot != null)
        {
            axis = pivot.right * rotaxis.x + pivot.up * rotaxis.y + pivot.forward * rotaxis.z;
        }
        else
        {
            axis = t.right * rotaxis.x + t.up * rotaxis.y + t.forward * rotaxis.z;
        }
        
        t.RotateAround(pivotPt, axis, amt );
    }
}