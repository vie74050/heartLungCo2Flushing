/* Extends flow meter control to add specific conditions for flow meter on hose */
using UnityEngine;
public class KnoControl_FlowMeter : KnobControl
{
    public Pivoter co2SourcePivoter; // reference to CO2 source pivoter to check if open
    public HoseFlow[] downstreamHoses;
    private void LateUpdate() {
        // check co2SourcePivoter IsOn state each frame
        SetEnabled(co2SourcePivoter.IsOn);
    }
    public override void HandleKnobUpdated()
    {
      
        // get value from flow meter knob
        float flowValue = GetNormalizedKnobValue();
        // set downstream hose flow based on knob value and CO2 source state
        foreach (var hose in downstreamHoses)
        {
            hose.SetFlow(co2SourcePivoter.IsOn ? flowValue : 0f);
        }
    }

    
}