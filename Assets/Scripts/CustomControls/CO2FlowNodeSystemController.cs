using UnityEngine;

/* on the Main object in the scene that manages the flow node system */
public class CO2FlowNodeSystemController : FlowNodeSystemController
{
    // For CO2 flushing, the Flow Meter Control is a Knob Dial Control that sets the initial flow rate
    [Tooltip("The Flow Meter Control that sets the flow rate")]
    public KnobDialControl_FlowMeter flowMeterControl;
    
    public override void UpdateFlowSystem()
    {
        flowMeterControl?.HandleKnobUpdated();
    }

    
}   