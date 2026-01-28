using UnityEngine;

public class CO2TankONOFF : Pivoter
{
    // The tank also controls the CO2 flow meter knob
    [Tooltip("Reference to the CO2 Flow Meter Knob Control")]
    public KnobDialControl_FlowMeter co2FlowMeterKnob;

    public override void OnTurnedOn()
    {
        base.OnTurnedOn();
        co2FlowMeterKnob.SetEnabled(true);
    }

    public override void OnTurnedOff()
    {
        base.OnTurnedOff();
        co2FlowMeterKnob.SetEnabled(false);
    }
}