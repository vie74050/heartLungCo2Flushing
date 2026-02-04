using UnityEngine;

public class IVFluidFlowNodeSystemController : FlowNodeSystemController
{

    [Tooltip("WarningTextControl for IV Fluid Hose to show warnings")]
    public AlertTextControl warningTextControl;
    private FlowNode sourceNode;
    
    void Start()
    {
        // find source node in children
        sourceNode = GetComponentInChildren<FlowNode>();
    }
    // Call this global when system changes to recalculate flow through system
    public override void UpdateFlowSystem()
    {
        sourceNode?.SetFlow(1f);
        
    }
    void LateUpdate() {
        // check conditions each frame
        
    }

}