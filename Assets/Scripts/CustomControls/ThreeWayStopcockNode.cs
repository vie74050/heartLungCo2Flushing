using UnityEngine;

public class ThreeWayStopcockNode : Stopcock
{
    // 3-way valve positions in order of 0, 1, 2, 3, where 
    // 0 is closed
    // 1 is up 
    // 2 is open 
    // 3 is down
    
    [Tooltip("Current state of the stopcock")]
    public int state;
    public override void SetFlow(float flow)
    {
       
        state = Mathf.RoundToInt(currentPositionIndex);
        // round to whole number for stability
        //Debug.Log("ThreeWayStopcockNode SetFlow called. State: " + state + " Flow: " + flow);

        switch (currentPositionIndex)
        {
            case 0:              
                downstreamFlowNodes[1]?.SetFlow(0f);
                downstreamFlowNodes[3]?.SetFlow(0f);
                break;
            case 1:        
                downstreamFlowNodes[1]?.SetFlow(flow);              
                downstreamFlowNodes[3]?.SetFlow(0f);
                break;
            case 2: 
                downstreamFlowNodes[1]?.SetFlow(flow);
                downstreamFlowNodes[3]?.SetFlow(flow);
                break;
            case 3:
                downstreamFlowNodes[1]?.SetFlow(0f);
                downstreamFlowNodes[3]?.SetFlow(flow);
                break;
            default:
                Debug.LogWarning("Invalid valve position");
                break;
        }
    }

}