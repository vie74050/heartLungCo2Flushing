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

    public Texture2D cursorTexture;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    void OnMouseEnter()
    {
        Cursor.SetCursor(cursorTexture, hotSpot, cursorMode);
    }

    void OnMouseExit()
    {
        // Pass 'null' to the texture parameter to use the default system cursor.
        Cursor.SetCursor(null, Vector2.zero, cursorMode);
    }

    void OnMouseDown()
    {
        // Requires web handling code to process this message
        #if UNITY_WEBGL && !UNITY_EDITOR
			BrowserSelect(transform.name);
        #endif
    }
}