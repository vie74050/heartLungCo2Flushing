using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Knob Control Script
 * 
 * This script allows a knob object to be rotated by mouse drag and moves a target object along a specified axis based on the knob's rotation.
 * The knob rotation is clamped between minAngle and maxAngle, and the target object's position is updated accordingly.
 * 
 * Attach this script to the knob GameObject and assign the target object and movement axis in the inspector.
 */
public class Knob : MonoBehaviour
{
    [Tooltip("The target object to move based on knob rotation")]
    public Transform targetObject; 
    [Tooltip("The axis along which to move the target object moves")]
    public Vector3 targetAxis; 
    [Tooltip("The min and max angles for the knob rotation")]
    public float minAngle = -90;
    [Tooltip("The min and max angles for the knob rotation")]
    public float maxAngle = 90;
    [Tooltip("The min and max values for the target object movement")]
    public float minValue;  
    public float maxValue; 
    [Tooltip("Enable or disable the knob control")]
    public bool isActive = true;
    [Tooltip("Optional Renderer for the indicator on the knob - change material color based on knobEnabled state")]
    public Renderer indicatorRenderer;

    private Vector3 initTargetPosition;
    private Color indicatorColor;
    private KGFOrbitCam camsettings;
    private InteractableCursor cursorSetter; // optional to change cursor if disabled
    private Texture2D defaultCursorTexture;

    void Start()
    {
        // get cam settings ref -- to override panning when dragging
		camsettings = Camera.main.GetComponent<KGFOrbitCam>();
        cursorSetter = GetComponent<InteractableCursor>();
        if (cursorSetter != null)
        {
            defaultCursorTexture = cursorSetter.cursorTexture;
        }

        // save the initial position of the target object
        initTargetPosition = new Vector3(targetObject.localPosition.x, targetObject.localPosition.y, targetObject.localPosition.z);
        // save the initial color of the indicator
        if (indicatorRenderer != null)
        {
            indicatorColor = indicatorRenderer.material.GetColor("_Color");
        }
    }
    void Update()
    {
        SetEnabled(isActive);
    }
    private void OnMouseOver() 
    {
        if (isActive && Input.GetMouseButton(0))
        {
            camsettings.SetPanningEnable(false);
            UpdateKnobRotation();
            UpdateTargetPosition();
        }else
        {
            camsettings.SetPanningEnable(true);
        }
      
    }

    private void OnMouseUp() 
    {
        camsettings.SetPanningEnable(true);    
    }   
    // Update Knob Rotation 
    private void UpdateKnobRotation()
    {
        //get drag direction left or right
        float dragDirection = Input.GetAxis("Mouse X");
        //rotate knob with min and max angle
        if (dragDirection > 0)
        {
            if (gameObject.transform.localEulerAngles.z < maxAngle )
            {
                gameObject.transform.Rotate(0, 0, 1);
            }
                   }
        else if (dragDirection < 0)
        {
            // NB clamp rotation to positive values
            if (gameObject.transform.localEulerAngles.z > minAngle && gameObject.transform.localEulerAngles.z > 1)
            {
                gameObject.transform.Rotate(0, 0, -1);
            }
        }
    }


    // Update Target Position based on knob angle
    private void UpdateTargetPosition()
    {
        // move targetObject along targetAxis based on the rotation of the knob
        float angle = Mathf.Clamp(gameObject.transform.localEulerAngles.z, minAngle, maxAngle);
        float value = angle / (maxAngle - minAngle) * (maxValue - minValue);
        targetObject.localPosition = initTargetPosition + targetAxis * value;
    }

    public float GetKnobValue()
    {
        float angle = Mathf.Clamp(gameObject.transform.localEulerAngles.z, minAngle, maxAngle);
        float value = angle / (maxAngle - minAngle) * (maxValue - minValue);
        return value;
    }

    public void SetEnabled(bool enabled)
    {
        isActive = enabled;

        if (!isActive)
        {
            // reset knob rotation and target position and value to min
            gameObject.transform.localEulerAngles = new Vector3(0, 0, minAngle);
            targetObject.localPosition = initTargetPosition + targetAxis * minValue;    
            // update target position to min value
            UpdateTargetPosition();
        }

        // Update indicator color based on knobEnabled state
        if (indicatorRenderer != null)
        {
            Color currentColor = indicatorRenderer.material.GetColor("_Color");
            Color targetColor = isActive ? indicatorColor : Color.red;
            if (currentColor != targetColor)
            {
                indicatorRenderer.material.SetColor("_Color", targetColor);
            }
        }
        // Update cursor if cursorSetter is available
        if (cursorSetter != null)
        {
            if (isActive)
            {
                cursorSetter.SetCursorTexture(defaultCursorTexture);
            }
            else
            {
                cursorSetter.SetCursorTexture(null);
            }
        }
    }

    public void ResetAll()
    {
        // reset knob rotation and target position to initial state
        gameObject.transform.localEulerAngles = new Vector3(0, 0, minAngle);
        targetObject.localPosition = initTargetPosition;
        // restore indicator color
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material.SetColor("_Color", indicatorColor);
        }
    }
}
