using UnityEngine;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Collider))]
/* Attach this script to a GameObject with a Collider for mouse event handling:

For WebGL: 
- mouse cursor changes when the mouse enters the object's area.
- ensure cursor size is 32x32 pixels for best results.
- send message to browser (for further web handling of mouse over event) --> see JSLibs
*/
public class Interactible : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void BrowserSelect(string str);
    [DllImport("__Internal")]
    private static extern void BrowserHover(string str);

    [Tooltip("Optional display name instead of transform name")]
	public string displayName = "";
    [Tooltip("Texture for the cursor when hovering over this object")]
    public Texture2D cursorTexture; // see Resources folder. Cursor should be 32x32 for web

    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    // List of ClickAction scripts attached to this GameObject
    private ClickAction[] clickActions;

    private void Awake() {
        SetCursorTexture(cursorTexture);
        clickActions = GetComponents<ClickAction>();
    }
    void OnMouseEnter()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
        // Requires web handling code to process this message
        #if UNITY_WEBGL && !UNITY_EDITOR
            string objectName = GetLabelName();
            BrowserHover(objectName);
        #endif
    }

    void OnMouseExit()
    {
        // Pass 'null' to the texture parameter to use the default system cursor.
        Cursor.SetCursor(null, Vector2.zero, cursorMode);

        // Requires web handling code to process this message
        #if UNITY_WEBGL && !UNITY_EDITOR
            BrowserHover("");
        #endif
    }
    
    public void OnMouseDown()
    {
        // Requires web handling code to process this message
        #if UNITY_WEBGL && !UNITY_EDITOR
            string objectName = GetLabelName();
			BrowserSelect(objectName);
        #endif
        //Debug.Log("Clicked on " + transform.name);

        if (clickActions != null)
        {
            foreach (ClickAction action in clickActions)
            {
                if (action != null)
                {
                    action.OnClick();
                }
            }
        }
    }

    public void SetCursorTexture(Texture2D newTexture)
    {
        if (newTexture == null)
        {
            cursorTexture = Resources.Load<Texture2D>("Texture2D Sprites and Cursors/cursor_pointer");
        }else
        {
            cursorTexture = newTexture;
        }

    }
    public string GetLabelName()
    {
        return displayName != "" ? displayName : (  transform.name == "Label" ? transform.parent.name : transform.name );
    }

}

