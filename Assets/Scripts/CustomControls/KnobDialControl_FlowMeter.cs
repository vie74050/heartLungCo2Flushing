/* Extends flow meter control to add specific conditions for flow meter on hose */
using UnityEngine;
public class KnobDialControl_FlowMeter : KnobDialControl
{
    [Header("CO2 Flow Meter Specific Settings")]
    public Pivoter co2SourcePivoter; // reference to CO2 source pivoter to check if open
    public FlowNodeHose downstreamHose;

    public override void HandleKnobUpdated()
    {
        base.HandleKnobUpdated();

        // get value from flow meter knob
        float flowValue = GetNormalizedKnobValue();
        // set downstream hose flow based on knob value and CO2 source state
        downstreamHose?.SetFlow(co2SourcePivoter.IsOn ? flowValue : 0f);
        
        //Debug.Log("Downstream hose flow set to: " + (co2SourcePivoter.IsOn ? flowValue : 0f));
    }

    
}