using UnityEngine;

/* on the Main object in the scene that manages the flow node system */
public class CO2FlowNodeSystemController : FlowNodeSystemController
{
    // For CO2 flushing, the Flow Meter Control is a Knob Dial Control that sets the initial flow rate
    [Tooltip("The Flow Meter Control that sets the flow rate")]
    public KnobDialControl_FlowMeter flowMeterControl;

    [Tooltip("WarningTextControl for CO2 Hose to show warnings")]
    public WarningTextControl_CO2Hose warningTextControl;

    // Call this global when system changes to recaulculate flow through system
    public override void UpdateFlowSystem()
    {
        flowMeterControl?.HandleKnobUpdated();
        
    }
    void LateUpdate() {
        // check conditions each frame
        warningTextControl?.CheckWarningConditions();
    }

}   