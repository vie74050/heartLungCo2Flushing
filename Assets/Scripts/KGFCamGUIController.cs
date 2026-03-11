/* Creates buttons to control the KGF Orbit Cam:
- Reset View
- Rotate Left/Right
- Rotate Up/Down
- Pan Left/Right
- Pan Up/Down
- Zoom In/Out
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KGFCamGUIController : MonoBehaviour
{
    public GUISkin guistyle;
    private KGFOrbitCam camsettings;
    private float rotationVertAmt = 0;
    private float rotationHorzAmt = 0;
    private float panHorizontalAmt = 0;
    private float panVerticalAmt = 0;
    private float zoomAmt = 0;

    public enum CameraDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    void Start()
    {
        camsettings = KGFAccessor.GetObject<KGFOrbitCam>();
        ResetView();
    }

    void ResetView()
    {
        camsettings.ApplyStartValues();

        rotationHorzAmt = camsettings.GetRotationHorizontalCurrent();
        rotationVertAmt = camsettings.GetRotationVerticalCurrent();
        panHorizontalAmt = camsettings.itsPanningOffset.x;
        panVerticalAmt = camsettings.itsPanningOffset.y;
    }

    void RotateCamera(CameraDirection direction)
    {
        float sensitivity = 10f; // adjust as needed
        
        switch (direction)
        {
            case CameraDirection.Left:
            case CameraDirection.Right:
                rotationHorzAmt -= direction == CameraDirection.Left ? -sensitivity : direction == CameraDirection.Right ? sensitivity : 0;
                
                camsettings.SetRotationHorizontal(rotationHorzAmt);

                //Debug.Log("Horizontal Rotation: " + rotationHorzAmt);
                break;

            case CameraDirection.Up:    
            case CameraDirection.Down:
                float start = camsettings.itsRotation.itsVertical.itsStartValue;
                float max = start + camsettings.itsRotation.itsVertical.itsUpLimit;
                float min = start - camsettings.itsRotation.itsVertical.itsDownLimit;
                rotationVertAmt -= direction == CameraDirection.Up ? -sensitivity : direction == CameraDirection.Down ? sensitivity : 0;

                rotationVertAmt = Mathf.Clamp(rotationVertAmt, min, max);
                camsettings.SetRotationVertical(rotationVertAmt);
                //Debug.Log("Vertical Rotation: " + rotationVertAmt);
                break;
        }
    }
   
    void PanCamera(CameraDirection direction)
    {
        float sensitivity = camsettings.GetPanningSpeed(); 
        switch (direction)
        {
            case CameraDirection.Left:
            case CameraDirection.Right:
                
                panHorizontalAmt = direction == CameraDirection.Left ? -sensitivity : direction == CameraDirection.Right ? sensitivity : 0;
                
                Vector3 aDelta= transform.right.normalized * panHorizontalAmt;
                camsettings.itsPanningOffset += aDelta;
                camsettings.DampPanningDirection();
      
                break;
            case CameraDirection.Up:
            case CameraDirection.Down:
                panVerticalAmt = direction == CameraDirection.Up ? sensitivity : direction == CameraDirection.Down ? -sensitivity : 0;
                
                Vector3 aDeltaV = transform.up.normalized * panVerticalAmt;
                camsettings.itsPanningOffset += aDeltaV;
                camsettings.DampPanningDirection();

                break;
        }
    
    }

    void ZoomCamera(float amount)
    {
        float sensitivity = camsettings.GetZoomSpeed();
        float min = camsettings.GetZoomMinLimit();
        float max = camsettings.GetZoomMaxLimit();
        
        zoomAmt = amount * sensitivity + camsettings.GetZoom();
        zoomAmt = Mathf.Clamp(zoomAmt, min, max);   
        camsettings.SetZoom(zoomAmt);

        //Debug.Log("Zoom: " + zoomAmt);
    }

    float width = 150f;
    float height = 210f;
    bool showMenu = true;
    // create the GUILayout buttons
    void OnGUI()
    {
        GUI.skin = guistyle;

        // Calculate vertical centering
        float x = 0;
        float y = (Screen.height - height) / 2f;     
        
        GUILayout.BeginArea(new Rect(x, y, width, height));
        showMenu = GUILayout.Toggle(showMenu, "");
        
        if (showMenu)
        {
            GUILayout.BeginVertical(guistyle.box);
            if (GUILayout.Button("Reset View"))
            {
                ResetView();
            }

            GUILayout.Label("Orbit");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                RotateCamera(CameraDirection.Left);
            }
            if (GUILayout.Button(">"))
            {
                RotateCamera(CameraDirection.Right);
            }
            if (GUILayout.Button("^"))
            {
                RotateCamera(CameraDirection.Up);
            }
            if (GUILayout.Button("v"))
            {
                RotateCamera(CameraDirection.Down);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Move");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("<"))
            {
                PanCamera(CameraDirection.Left);
            }
            if (GUILayout.Button(">"))
            {
                PanCamera(CameraDirection.Right);
            }
            if (GUILayout.Button("^"))
            {
                PanCamera(CameraDirection.Up);
            }
            if (GUILayout.Button("v"))
            {
                PanCamera(CameraDirection.Down);
            }
            GUILayout.EndHorizontal();

            GUILayout.Label("Zoom");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("+"))
            {
                ZoomCamera(-1f);
            }
            if (GUILayout.Button("-"))
            {
                ZoomCamera(1f);
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
                
        GUILayout.EndArea();
    }
}