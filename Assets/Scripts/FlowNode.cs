using UnityEngine;

public abstract class FlowNode : MonoBehaviour
{
    // This is a marker interface for flow nodes like valves and stopcocks
    [Tooltip("Downstream hose dependents, optional.")]
    public FlowNode[] downstreamFlowNodes; // propogate flow if needed
    public abstract void SetFlow(float flow);
}