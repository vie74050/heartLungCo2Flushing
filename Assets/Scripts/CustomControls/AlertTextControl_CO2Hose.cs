/* 
*  Extends WarningTextControl to add specific conditions for CO2 hose
*/
using UnityEngine;

public class AlertTextControl_CO2Hose : AlertTextControl
{
    [Tooltip("FlowMeterControl that sets flow rate")]
    public KnobDialControl_FlowMeter flowMeterControl;
    [Tooltip("Reference to CO2Hose supply line")]
    public FlowNodeHose supplyLine;

    [Tooltip("Reference to stopcock to check if open")]
    public Stopcock stopcock;

    [Tooltip("Reference to dropable script on hose end")]
    public SO_Dropable hoseEndDropable;

    [Tooltip("The parent transform that hose should connect to")]
    public Transform targetDropParent;

    [Tooltip("The terminal tubes for each route to check if vented properly")]
    public Transform[] endTerminalsToCheck;

    private string[] warnings = {
        "Warning: CO2 is flowing but not connected to system! Drag tube end to correct port!",
        "Warning: CO2 is flowing but stopcock is closed! Click the stopcock to rotate it!",
        "Warning: CO2 cannot vent out properly!"
    };

    // Called from system controller to check warning conditions when flow changes
    public void CheckWarningConditions()
    {
        string warning = "";
        // get hoseEndDropable current parent to see if it is targetDropParent
        string currentParentName = hoseEndDropable.GetCurrentParentName();
        bool isFlow = supplyLine.normalizedFlowRate > 0.0f;
        bool isConnected = currentParentName == targetDropParent.name;
        bool isStopcockOpen = stopcock.GetCurrentPositionIndex() > 0; // assuming index 0 is closed
        bool isFlowMeterOn = flowMeterControl?flowMeterControl.GetNormalizedKnobValue() > 0.0f : false;
        
        if (isFlow && !isConnected)
        {
            warning = warnings[0];
        }
        else if (isFlow && !isStopcockOpen)
        {
            warning = warnings[1];
           
        }
        
        // Check if CO2 can vent out properly
        else if (isFlow)
        {
            bool allZeroFlow = true;
            foreach (Transform terminal in endTerminalsToCheck)
            {
                FlowNodeHose endHose = terminal.GetComponent<FlowNodeHose>();
                if (endHose != null && endHose.normalizedFlowRate > 0.0f)
                {
                    allZeroFlow = false;
                    break;
                }
            }

            if (allZeroFlow)
            {
                warning = warnings[2];
            }
        }   

        if (isFlowMeterOn && supplyLine.isClamped)
        {
            // supply line is connected and on but clamped
            //Debug.Log("CO2 Hose connected and stopcock open but no flow - likely clamped.");
            warning = warnings[2];
        }

        UpdateWarningText(warning);
        
    }
}