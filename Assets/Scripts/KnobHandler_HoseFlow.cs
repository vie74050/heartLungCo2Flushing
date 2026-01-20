/* Add to game object with knob to handle hose flow control 
* Require:
- KnobControl.cs component on the same game object to provide knob value
- HoseFlow.cs component on the Target hose object to control flow rate

Optional: Set Pivotor to set knob enabled state
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KnobHandler_HoseFlow : MonoBehaviour
{
    [Tooltip("The HoseFlow component to control")]
    public HoseFlow hoseFlow; 
    [Tooltip("Maximum flow rate corresponding to max knob value")]
    public float maxFlowRate = 12f; 
    [Tooltip("Optional Pivot object to enable/disable knob control")]
    public Pivoter pivotObject;

    private Knob knobControl;

    void Start()
    {
        knobControl = GetComponent<Knob>();
        if (knobControl == null)
        {
            Debug.LogError("Knob component not found on the game object.");
        }
        if (hoseFlow == null)
        {
            Debug.LogError("HoseFlow component not assigned.");
        }
        if (pivotObject != null)
        {
            knobControl.knobEnabled = pivotObject.isOpen;
        }
    }

    void Update()
    {
        if (pivotObject != null)
        {
            knobControl.knobEnabled = pivotObject.isOpen;
        }

        if (knobControl != null && hoseFlow != null)
        {
            float knobValue = knobControl.GetKnobValue();
            float flowRate = Mathf.Lerp(0f, maxFlowRate, knobValue / (knobControl.maxValue - knobControl.minValue));

            // normalize flowrate sent to hose 0-1
            float normalizedFlowRate = flowRate / maxFlowRate;
            hoseFlow.SetFlow(normalizedFlowRate);
        }
    }
}
