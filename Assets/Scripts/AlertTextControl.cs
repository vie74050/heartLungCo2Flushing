/* Attach to Canvas with TextMesh Pro text child component.
Expose methods to show and hide warning text & update the text displayed. */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
[RequireComponent(typeof(Canvas))]
public class AlertTextControl : MonoBehaviour
{
    [Tooltip("TextMesh Pro text component for warning text display")]
    public TextMeshProUGUI alertText;
    [Tooltip("Whether to show warning on start")]
    public bool showOnStart = false;
    [Tooltip("Optional: text to show OnClick")]
    public string onClickText = "";

    private Canvas canvas;

    protected void Start()
    {
        // try to get component from children
        alertText = GetComponentInChildren<TextMeshProUGUI>();
        canvas = GetComponent<Canvas>();

        if (alertText == null)
        {
            Debug.LogError("AlertTextControl: TextMeshProUGUI component not assigned.");
            
        }
        else
        {
           if (showOnStart)
           {
               ShowAlert();
           }
           else
           {
               HideAlert();
           }
        }
    }

    protected void Update()
    {
        // make it so text always facing camera (World Space canvas)
        if (alertText != null)
        {
            canvas.transform.rotation = Camera.main.transform.rotation;
        }
    }

    // Show alert text
    private void ShowAlert()
    {
        gameObject.SetActive(true);
    }

    // Hide alert text
    private void HideAlert()
    {
        gameObject.SetActive(false);
    }

    // Update the alert text content, send empty string to hide
    public void UpdateWarningText(string newText)
    {
        if (newText == "")
        {
            // empty text, hide warning
            HideAlert();
            return;
        }
        if (alertText != null)
        {
            alertText.text = newText;
            ShowAlert();
            
        }
    }
}

               