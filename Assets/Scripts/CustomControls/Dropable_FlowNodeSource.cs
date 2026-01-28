using UnityEngine;

public class Dropable_SourceNode : SO_Dropable
{
    // This class extends the dropable functionality so on drop 
    // it can set downstream flow nodes of the supply line

    public KnobDialControl_FlowMeter flowMeterControl; // Reference to the flow meter control
    public FlowNode flowNode_supplyLine; // Reference to the FlowNode component to update
    [Tooltip("Downstream hose dependents to connect to supply line when dropped.")]
    public FlowNode[] downstreamFlowNodes; 

    // Called when the object is dropped
    protected override void OnDrop()
    {
        base.OnDrop();
        // sets the downstream flow nodes to active when this source node is dropped
        if (flowNode_supplyLine != null)
        {
            // connect the downstream flow nodes for the supply line
            flowNode_supplyLine.downstreamFlowNodes = downstreamFlowNodes;
            // Update flow system 
            flowMeterControl?.HandleKnobUpdated(); 
        }
        
    }

}