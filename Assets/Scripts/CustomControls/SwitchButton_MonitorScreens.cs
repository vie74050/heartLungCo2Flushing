
/// <summary>
/// This script handles the behaviour of heart lung machine ON/OFF buttons on the door panel.
/// </summary>
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(GlobalMaterialColorChanger))]
public class MonitorScreensONOFF_button : SwitchButtonsToggle
{
    // for changing the color of the monitor screens when turned on/off
    private GlobalMaterialColorChanger colorChanger;    
    private Color originalColor;
    
    private void Awake() {
        colorChanger = GetComponent<GlobalMaterialColorChanger>();
        originalColor = colorChanger.originalColor;
    }
    protected override void Start()
    {
        base.Start();
    }
    public override void OnMouseDown() 
    {
        base.OnMouseDown();
    }
    public override void OnTurnedOn()
    {
        colorChanger.ChangeGlobalMaterialColor();
        base.OnTurnedOn();
    }
    public override void OnTurnedOff()
    {
        colorChanger.ResetMaterialColor();
        base.OnTurnedOff();
    }

}