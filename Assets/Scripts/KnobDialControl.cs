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
public class KnobDialControl : MonoBehaviour
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
    // know should be set up to rotate around local Z axis
    private float initKnobRotationZ;
    private Color indicatorColor;
    private KGFOrbitCam camsettings;
    private bool panningInitEnabled;
    private Interactible cursorSetter; // optional to change cursor if disabled
    private Texture2D defaultCursorTexture;

    void Start()
    {
        // get cam settings ref -- to override panning when dragging
		camsettings = Camera.main.GetComponent<KGFOrbitCam>();
        if (camsettings != null)
        {
			var p = camsettings.itsPanning;
			panningInitEnabled = p.itsLeftRight.itsEnable || p.itsUpDown.itsEnable;
		}
        cursorSetter = GetComponent<Interactible>();
        if (cursorSetter != null)
        {
            defaultCursorTexture = cursorSetter.cursorTexture;
        }

        // save the initial rotation of the knob
        initKnobRotationZ = gameObject.transform.localEulerAngles.z;

        // save the initial position of the target object
        initTargetPosition = new Vector3(targetObject.localPosition.x, targetObject.localPosition.y, targetObject.localPosition.z);
        // save the initial color of the indicator
        if (indicatorRenderer != null)
        {
            indicatorColor = indicatorRenderer.material.GetColor("_Color");
        }

        UpdateVisuals();
    }
   
    private void OnMouseOver() 
    {
        if (isActive && Input.GetMouseButton(0))
        {
            camsettings.SetPanningEnable(false);
            UpdateKnobRotationMouseMove();
            UpdateTargetPosition();
        }else
        {
            camsettings.SetPanningEnable(panningInitEnabled);
        }
      
    }

    private void OnMouseUp() 
    {
        camsettings.SetPanningEnable(panningInitEnabled);    
    }  
    private void OnMouseExit() 
    {
        camsettings.SetPanningEnable(panningInitEnabled);    
    } 
    // Update Knob Rotation around local Z axis based on mouse drag
    private void UpdateKnobRotationMouseMove()
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


    // Update Target Position based on knob local z angle
    private void UpdateTargetPosition()
    {
        // move targetObject along targetAxis based on the rotation of the knob
        float value = GetKnobValue();
        targetObject.localPosition = initTargetPosition + targetAxis * value;
        HandleKnobUpdated();
    }

    private float GetKnobValue()
    {
        float angle = Mathf.Clamp(gameObject.transform.localEulerAngles.z, minAngle, maxAngle);
        float value = angle / (maxAngle - minAngle) * (maxValue - minValue);
        return value;
    }
    private void UpdateVisuals()
    {
        
        if (!isActive)
        {
            // reset knob rotation and target position and value to min
            gameObject.transform.localEulerAngles = new Vector3(0, 0, minAngle);   
            // update target position 
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
    // returns normalized knob value between 0 and 1
    public float GetNormalizedKnobValue()
    {
        float angle = Mathf.Clamp(gameObject.transform.localEulerAngles.z, minAngle, maxAngle);
        float normalizedValue = (angle - minAngle) / (maxAngle - minAngle);
        return normalizedValue;
    }

    public void Set_isActive(bool enabled)
    {
        // only update if state changed
        if (isActive == enabled) return;

        isActive = enabled;
        UpdateVisuals();
    }

    public void ResetAll()
    {
        // reset knob rotation and target position to initial state
        gameObject.transform.localEulerAngles = new Vector3(0, 0, initKnobRotationZ);
        UpdateTargetPosition();
        // restore indicator color
        if (indicatorRenderer != null)
        {
            indicatorRenderer.material.SetColor("_Color", indicatorColor);
        }
    }

    public virtual void HandleKnobUpdated()
    {
        
    }
}
