/* Controller for game object that behaves like a stopcock
   - Rotates to preset angles on each mouse click
   - Keeps track of current position index
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopcock : FlowNode
{
    [Tooltip("The Flow Meter Control that sets the flow rate")]
    public KnobDialControl_FlowMeter flowMeterControl; // Reference to the flow meter control
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
        flowMeterControl?.HandleKnobUpdated();     
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
        for (int i = 0; i < downstreamFlowNodes.Length; i++)
        {
            if (i == currentPositionIndex)
            {
                downstreamFlowNodes[i]?.SetFlow(flow);
            }
            else
            {
                downstreamFlowNodes[i]?.SetFlow(0f);
            }
        } 
    }
}