/* Extends flow meter control to add specific conditions for flow meter on hose */
using UnityEngine;
public class KnobDialControl_FlowMeter : KnobDialControl
{
    [Header("CO2 Flow Meter Specific Settings")]
    [Tooltip("Reference downstream hose to set flow on update flow")]
    public FlowNodeHose downstreamHose;

    public override void HandleKnobUpdated()
    {
        base.HandleKnobUpdated();

        // get value from flow meter knob
        float flowValue = GetNormalizedKnobValue();
        // set downstream hose flow based on knob value and CO2 source state
        downstreamHose?.SetFlow(isActive? flowValue : 0f);
        
    }

    
}