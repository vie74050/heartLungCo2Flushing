/* Add this to FlowNodeHose to set flow of another FlowNode without triggering downstream updates
i.e. for instances when this FlowNodeHose is a downstream node of the targetHose, to prevent infinite recursion. 
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// REQUIRE FlowNodeHose component 
[RequireComponent(typeof(FlowNodeHose))]
public class FlowNodeClampOverride : MonoBehaviour
{
    [Tooltip("The FlowNodeHose to override flow for.")]
    public FlowNodeHose targetHose;
  
    private FlowNodeHose thisHose;

    void Start()
    {
        // get reference for this FlowNodeHose component
        thisHose = GetComponent<FlowNodeHose>();
        
        if (targetHose == null)
        {
            Debug.LogError("FlowNodeClampOverride requires a targetHose reference.");
        }

    }
    private void LateUpdate()
    {
        if (targetHose != null )
        {

            // check if thisHose normalized flow rate is 0, if so, set targetHose flow to flowOverride
            if (thisHose.isClamped && targetHose.normalizedFlowRate > 0f)
            {
                targetHose.SetMyFlowOnly(0f);  
            }

            if (thisHose.isClamped == false )
            {
                thisHose.UpdateFlowSystem();
            }
           
        }
    }
}