/* A ClickAction handler that sets the clamp state of a FlowNodeHose */
using UnityEngine;

public class FlowNodeClampControl : ClickAction
{
    [Header("FlowNode Clamp Control Settings")]
    [Tooltip("The target FlowNodeHose to set flow clamp on")]
    public FlowNodeHose targetHose;

    [Tooltip("The clamp object Transform reference")]
    public Transform clampObjectRefTn;

    void Start() {
        // for linked ON OFF buttons, ensure they have the same parent 
        if (clampObjectRefTn == null)
            clampObjectRefTn = transform.parent; 
        
        if (isOn)
        {
            targetHose.SetClamp(clampObjectRefTn.gameObject, true);
        }
        else
        {
            targetHose.SetClamp(clampObjectRefTn.gameObject, false);
        }
    }
    public override void OnClick()
    {
        if (targetHose == null)
        {
            Debug.LogWarning("FlowNodeClampControl: Missing targetHose reference.");
            return;
        }

        switch (onClickActiveMode)
        {
            case GameObjectActiveMode.SetTrue:
                targetHose.SetClamp(clampObjectRefTn.gameObject, true);
                break;
            case GameObjectActiveMode.SetFalse:
                targetHose.SetClamp(clampObjectRefTn.gameObject, false);
                break;
            case GameObjectActiveMode.Toggle:
                bool isClamped = targetHose.isClamped;
                targetHose.SetClamp(clampObjectRefTn.gameObject, !isClamped);
                break;
        }

    }

    
}