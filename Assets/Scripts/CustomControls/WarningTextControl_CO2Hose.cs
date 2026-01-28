/* 
*  Extends WarningTextControl to add specific conditions for CO2 hose
*/
using UnityEngine;

public class WarningTextControl_CO2Hose : WarningTextControl
{
    // extends class to add conditions for when to show warnings
    public FlowNodeHose hose;               // reference to CO2Hose script to get hose flow rate
    public Stopcock stopcock;           // reference to stopcock to check if open
    public SO_Dropable hoseEndDropable; // reference to dropable script on hose end
    public Transform targetDropParent;  // the parent transform when tank is dropped correctly

    private string warning1 = "Warning: CO2 is flowing but not connected to system!";
    private string warning2 = "Warning: CO2 is flowing but stopcock is closed!";

    private void LateUpdate() {
            // check conditions each frame
            CheckWarningConditions();
    }
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
            warning = warning1;
        }
        else if (isFlow && !isStopcockOpen)
        {
            warning = warning2;
        }
        UpdateWarningText(warning);
        
    }
}