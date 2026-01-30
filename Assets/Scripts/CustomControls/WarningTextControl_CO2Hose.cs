/* 
*  Extends WarningTextControl to add specific conditions for CO2 hose
*/
using UnityEngine;

public class WarningTextControl_CO2Hose : WarningTextControl
{
    [Tooltip("Reference to CO2Hose script to get hose flow rate")]
    public FlowNodeHose hose;

    [Tooltip("Reference to stopcock to check if open")]
    public Stopcock stopcock;

    [Tooltip("Reference to dropable script on hose end")]
    public SO_Dropable hoseEndDropable;

    [Tooltip("The parent transform that hose should connect to")]
    public Transform targetDropParent;

    [Tooltip("The terminal tubes for each route to check if vented properly")]
    public Transform[] endTerminalsToCheck;

    private string[] warnings = {
        "Warning: CO2 is flowing but not connected to system!",
        "Warning: CO2 is flowing but stopcock is closed!",
        "Warning: CO2 cannot vent out properly!"
    };

    // Called from system controller to check warning conditions when flow changes
    public void CheckWarningConditions()
    {
        string warning = "";
        // get hoseEndDropable current parent to see if it is targetDropParent
        string currentParentName = hoseEndDropable.GetCurrentParentName();
        bool isFlow = hose.normalizedFlowRate > 0.01f;
        bool isConnected = currentParentName == targetDropParent.name;
        bool isStopcockOpen = stopcock.GetCurrentPositionIndex() > 0; // assuming index 0 is closed

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
                if (endHose != null && endHose.normalizedFlowRate > 0.01f)
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

        UpdateWarningText(warning);
        
    }
}