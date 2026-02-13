using UnityEngine;

public class Dropable_SourceNode : SO_Dropable
{
    // This class extends the dropable functionality so on drop 
    [Header("Dropable_SourceNode specific settings")]
    // it can set downstream flow nodes of the supply line
    [Tooltip("The Flow Meter Control that sets the flow rate")]
    public FlowNodeSystemController flowSystemController; 
    [Tooltip("The FlowNode component representing the supply line")]
    public FlowNode flowNode_supplyLine; 
    [Tooltip("Downstream hose dependents to connect to supply line when dropped.")]
    public FlowNode[] downstreamFlowNodes; 
    [Tooltip("Colliders to disable on drop")]
    public Collider[] collidersToDisableOnDrop;

    protected override void OnMouseDrag()
    {
        base.OnMouseDrag();
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

        // disable it's collider so doesn't hide filter collider
        foreach (Collider col in collidersToDisableOnDrop)
        {
            if (col)
            {
                col.enabled = false;
            }
        }
        
    }
}