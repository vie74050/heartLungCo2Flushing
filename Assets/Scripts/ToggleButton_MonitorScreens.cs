using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(GlobalMaterialColorChanger))]
public class ToggleButton_MonitorScreens : MonoBehaviour, IToggleButton, IPointerClickHandler
{
    [SerializeField] private Color onColor = Color.green;

    private GlobalMaterialColorChanger colorChanger;
    private bool isOn = false;
    private Color originalColor;

    public bool IsOn => isOn;

    private void Awake()
    {
        colorChanger = GetComponent<GlobalMaterialColorChanger>();
        originalColor = colorChanger.originalColor;
    }

    public void Toggle()
    {
        isOn = !isOn;

        if (isOn)
        {
            colorChanger.ChangeGlobalMaterialColor(onColor);
        }
        else
        {
            colorChanger.ChangeGlobalMaterialColor(originalColor);
        }
    }

    // Called when the object is clicked
    public void OnPointerClick(PointerEventData eventData)
    {
        Toggle();
    }

}