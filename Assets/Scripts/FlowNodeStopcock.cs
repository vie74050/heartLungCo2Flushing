/* Controller for game object that behaves like a stopcock
   - Rotates to preset angles on each mouse click
   - Keeps track of current position index
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopcock : FlowNode
{
    [Tooltip("Transform of the stopcock game object to rotate")]
    public Transform stopcockRotationTn;

    // assign down stream hoses in inspector in the same order as anglePositions
    // to which they correspond
    [Tooltip("The angles (in degrees) for each stopcock position")]
    public float[] anglePositions = new float[] {0f, -90f, 180f, 90f};
    [Tooltip("Starting position index")]
    public int startingPositionIndex = 0;

    protected int currentPositionIndex = 0;

    private void Start()
    {
        currentPositionIndex = startingPositionIndex;
        UpdateStopcock();
    }
    private void OnMouseDown()
    {
        RotateToNextPosition();
    }
    private void RotateToNextPosition()
    {
        currentPositionIndex = (currentPositionIndex + 1) % anglePositions.Length;
        UpdateStopcock();
    }

    private void UpdateStopcock()
    {
        // Rotate the stopcock to the correct angle
        float targetAngle = anglePositions[currentPositionIndex];
        stopcockRotationTn.localRotation = Quaternion.Euler(0, targetAngle, 0);

        // Update flow system   
        base.UpdateFlowSystem();     
        
    }

    // Public method to get the current position index
    public int GetCurrentPositionIndex()
    {
        return currentPositionIndex;
    }

    // called by upstream flow system to set flow
    public override void SetFlow(float flow)
    {
        // Propagate flow to downstream hoses based on current position
        if (downstreamFlowNodes == null) return;

        FlowNode currentNode = downstreamFlowNodes[currentPositionIndex];

        if (currentNode == null)
        {
            // set all lines to 0
            for (int i = 0; i < downstreamFlowNodes.Length; i++)
            {
                downstreamFlowNodes[i]?.SetFlow(0f);
                //Debug.Log("stopcock closed line " + i);
            }
        }else
        {
            currentNode.SetFlow(flow);
            // set all other lines to 0
            for (int i = 0; i < downstreamFlowNodes.Length; i++)
            {
                if (i != currentPositionIndex && downstreamFlowNodes[i] != currentNode)
                {
                    downstreamFlowNodes[i]?.SetFlow(0f);
                }
            }
        }
        
    }   
}