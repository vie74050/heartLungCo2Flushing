using UnityEngine;

public class CO2TankONOFF : Pivoter
{
    // The tank also controls CO2 flow meter knob enabled state
    [Tooltip("Reference to the CO2 Flow Meter Knob Control")]
    public KnobDialControl_FlowMeter co2FlowMeterKnob;
    [Tooltip("Reference to the CO2 Flow Meter Knob Control instructions")]
    public GameObject co2FlowMeterKnobInstructions;

    public override void OnMouseDown()
    {
        Toggle();
        mouseDowndT = 0; // reset mouse down timer to avoid conflict with dragging
    }
    public override void OnTurnedOn()
    {
        base.OnTurnedOn();
        co2FlowMeterKnob.Set_isActive(true);
        co2FlowMeterKnobInstructions.SetActive(true);
    }

    public override void OnTurnedOff()
    {
        base.OnTurnedOff();
        co2FlowMeterKnob.Set_isActive(false);
        co2FlowMeterKnobInstructions.SetActive(false);

    }
}