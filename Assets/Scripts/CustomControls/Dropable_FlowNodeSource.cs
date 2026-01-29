using UnityEngine;

public class Dropable_SourceNode : SO_Dropable
{
    // This class extends the dropable functionality so on drop 
    // it can set downstream flow nodes of the supply line
    [Tooltip("The Flow Meter Control that sets the flow rate")]
    public FlowNodeSystemController flowSystemController; 
    [Tooltip("The FlowNode component representing the supply line")]
    public FlowNode flowNode_supplyLine; 
    [Tooltip("Downstream hose dependents to connect to supply line when dropped.")]
    public FlowNode[] downstreamFlowNodes; 

    protected override void OnDrag()
    {
        base.OnDrag();
        flowNode_supplyLine.GetComponent<FlowNodeHose>()?.UpdateHoseLineRenderer();
    }
    protected override void OnDrop()
    {
        base.OnDrop();
        // sets the downstream flow nodes to active when this source node is dropped
        if (flowNode_supplyLine != null)
        {
            // connect the downstream flow nodes for the supply line
            flowNode_supplyLine.downstreamFlowNodes = downstreamFlowNodes;
            flowNode_supplyLine.GetComponent<FlowNodeHose>()?.UpdateHoseLineRenderer();
            // Update flow system   
            flowSystemController?.UpdateFlowSystem();     
        }
        
    }

}