/* Controller for game object that behaves like a stopcock
   - Rotates to preset angles on each mouse click
   - Keeps track of current position index
*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopcock : MonoBehaviour
{
    [Tooltip("The stopcock game object to rotate")]
    public GameObject stopcockObject;

    [Tooltip("The angles (in degrees) for each stopcock position")]
    public float[] anglePositions;

    private int currentPositionIndex = 0;

    private void Start()
    {
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
        stopcockObject.transform.localRotation = Quaternion.Euler(0, targetAngle, 0);

    }

    // Public method to get the current position index
    public int GetCurrentPositionIndex()
    {
        return currentPositionIndex;
    }
}