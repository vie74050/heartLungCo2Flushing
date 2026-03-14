using UnityEngine;

public abstract class FlowNode : MonoBehaviour
{
    [Header("Flow Node Settings")]

    // This is a marker interface for flow nodes like valves and stopcocks
    [Tooltip("Downstream hose dependents, optional.")]
    public FlowNode[] downstreamFlowNodes; // propogate flow if needed

    [Tooltip("The Flow Node System Controller. If unset, defaults to the one on the main camera.")]
    public FlowNodeSystemController flowSystemController;
    
    protected virtual void Awake()
    {
        // if unset, default to the on FlowNodeSystemController on main camera
        if (flowSystemController == null)
        {
            // check if there is a flowSystemController on this object first  
            flowSystemController = GetComponent<FlowNodeSystemController>();

            // then check main camera
            if (flowSystemController == null)
            {
                flowSystemController = Camera.main.GetComponent<FlowNodeSystemController>();
            }
        }
    }
    public void UpdateFlowSystem()
    {
        flowSystemController.UpdateFlowSystem();
    }
    public abstract void SetFlow(float flow);
}