/* Attach to Canvas with TextMesh Pro text child component.
Expose methods to show and hide warning text & update the text displayed. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(Canvas))]
public class WarningTextControl : MonoBehaviour
{
    [Tooltip("TextMesh Pro text component for warning text display")]
    public TextMeshProUGUI warningText;
    [Tooltip("Show on start")]
    public bool showOnStart = false;

    private Canvas canvas;

    protected void Start()
    {
        // try to get component from children
        warningText = GetComponentInChildren<TextMeshProUGUI>();
        canvas = GetComponent<Canvas>();

        if (warningText == null)
        {
            Debug.LogError("WarningTextControl: TextMeshProUGUI component not assigned.");
            
        }
        else
        {
           if (showOnStart)
           {
               ShowWarning();
           }
           else
           {
               HideWarning();
           }
        }
    }

    protected void Update()
    {
        // make it so text always facing camera 
        if (warningText != null)
        {
            canvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    // Show warning text
    private void ShowWarning()
    {
        if (warningText != null)
        {
            canvas.enabled = true;
        }
    }

    // Hide warning text
    private void HideWarning()
    {
        if (warningText != null)
        {
            canvas.enabled = false;
        }
    }

    // Update the warning text content, send empty string to hide
    public void UpdateWarningText(string newText)
    {
        if (newText == "")
        {
            // empty text, hide warning
            HideWarning();
            return;
        }
        if (warningText != null)
        {
            warningText.text = newText;
            ShowWarning();
            
        }
    }
}

               