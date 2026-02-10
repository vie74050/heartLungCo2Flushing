using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Pivoter.cs
 * 
 * Attach to an object you want to click on to pivot other objects around their pivots. e.g. scissors, doors, etc.
 * 
 * */

public class Pivoter : MonoBehaviour, ISwitch
{
    [Tooltip("Initial state of the pivoter")]
    public bool isOpen = false;

    [Tooltip("The objects to pivot around their pivots")]
    public PivotItem[] pvObjs;

    public bool IsOn { get => isOpen; set => isOpen = value; }
    public bool IsActive { get; set; } = true;
    
    protected float mouseDowndT = 0; // track how long mouse has been down to avoid conflict with mousedrag
    private int mouseDowndT_limit = 30;

    private void Start()
    {
        if (IsOn)
        {
            OnTurnedOn();
        }
       
    }
    public virtual void OnTurnedOn()
    {
        // open
        foreach (PivotItem pi in pvObjs)
        {
            pi.PivotAround(pi.max);
        }
        IsOn = true;
    }
    public virtual void OnTurnedOff()
    {
        // close
        foreach (PivotItem pi in pvObjs)
        {
            pi.PivotAround(-pi.max);
        }
        IsOn = false;
    }   
    private void Update()
    {
        if (Input.GetMouseButton(0))
        {
            mouseDowndT++;
        }
    }
    public virtual void OnMouseDown()
    {
        if (mouseDowndT < mouseDowndT_limit)
        {
            Toggle();
        }
        mouseDowndT = 0;
    }

    public void Toggle()
    {
        if (!IsActive) return;

        IsOn = !IsOn;

        if (IsOn)
        {
            OnTurnedOn();
        }
        else
        {
            OnTurnedOff();
        }
    }

}

[System.Serializable]
public class PivotItem
{
    public float max = 30;
    public Transform t;
    public Transform pivot; // optional, go that sets the pivot point, if null uses object's own position
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