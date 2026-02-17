using UnityEngine;

/* on the Main object in the scene that manages the flow node system */
public class CO2FlowNodeSystemController : FlowNodeSystemController
{
    // For CO2 flushing, the Flow Meter Control is a Knob Dial Control that sets the initial flow rate
    [Tooltip("The Flow Meter Control that sets the flow rate")]
    public KnobDialControl_FlowMeter flowMeterControl;

    [Tooltip("WarningTextControl for CO2 Hose to show warnings")]
    public AlertTextControl_CO2Hose warningTextControl;

    [Tooltip("ERC Clamp Control to toggle ERC clamp on/off")]
    public Interactible ercClampOFFControl;
    public Interactible ercClampONControl;

    public bool isERCClampOn = false; // track ERC clamp state

    // Call this global when system changes to recaulculate flow through system
    public override void UpdateFlowSystem()
    {
        flowMeterControl?.HandleKnobUpdated();
        
    }
    void LateUpdate() {
        // check conditions each frame
        warningTextControl?.CheckWarningConditions();
    }

    private void SetERCClamp()
    {
        // ensure both controls are active in hierarchy before sending click events to avoid errors
        if (ercClampOFFControl != null && ercClampONControl != null
            && ercClampOFFControl.gameObject.activeInHierarchy
            && ercClampONControl.gameObject.activeInHierarchy)
        {
            if (isERCClampOn)        {
                ercClampONControl?.OnMouseDown();
            }
            else
            {
                ercClampOFFControl?.OnMouseDown();
            }
        }
        
    }

    // Public methods for external WEBGL system panel
    public void SetERCClampOn() {
        isERCClampOn = true;
        SetERCClamp();
    }

    public void SetERCClampOff() {
        isERCClampOn = false;
        SetERCClamp();
    }
}   