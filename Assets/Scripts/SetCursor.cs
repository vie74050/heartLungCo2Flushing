using UnityEngine;
using System.Runtime.InteropServices;

[RequireComponent(typeof(Collider))]
/* Attach this script to a GameObject with a Collider for mouse event handling:

For WebGL: 
- mouse cursor changes when the mouse enters the object's area.
- ensure cursor size is 32x32 pixels for best results.
- send message to browser (for further web handling of mouse over event) --> see JSLibs
*/
public class InteractableCursor : MonoBehaviour
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

    private void Awake() {
        if (cursorTexture == null)
        {
            cursorTexture = Resources.Load<Texture2D>("Texture2D Cursors/cursor_pointer");
        }
    }
    void OnMouseEnter()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
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

    void OnMouseOver() {
        // Requires web handling code to process this message
        #if UNITY_WEBGL && !UNITY_EDITOR
            string objectName = getName();
            BrowserHover(objectName);
        #endif
    }
    
    void OnMouseDown()
    {
        // Requires web handling code to process this message
        #if UNITY_WEBGL && !UNITY_EDITOR
            string objectName = getName();
			BrowserSelect(objectName);
        #endif
        //Debug.Log("Clicked on " + transform.name);
    }

    private string getName()
    {
        return displayName != "" ? displayName : (  transform.name == "Label" ? transform.parent.name : transform.name );
    }

}