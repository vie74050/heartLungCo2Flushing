
/// <summary>
/// This script handles the behaviour of heart lung machine ON/OFF buttons on the door panel.
/// </summary>
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(GlobalMaterialColorChanger))]
public class MonitorScreensONOFF_button : MonoBehaviour, ISwitch
{
    [Tooltip("transform of the ON geometry to animate")]
    [SerializeField] private Transform ONBtnTransform;
    [Tooltip("transform of the OFF geometry to animate")]
    [SerializeField] private Transform OFFBtnTransform;
    [Tooltip("Initial state of the switch button")]
    public bool isOn = false;
    private GlobalMaterialColorChanger colorChanger;
    
    private Color originalColor;
    private Vector3 ONBtnOriginalPosition; 
    private Vector3 OFFBtnOriginalPosition;
    private Vector3 pressedOffset = new Vector3(0f, 0f, -1.1f);

    public bool IsOn { get => isOn; set => isOn = value; }
    public bool IsActive { get; set; } = true;


    private void Awake()
    {
        colorChanger = GetComponent<GlobalMaterialColorChanger>();
        originalColor = colorChanger.originalColor;
        ONBtnOriginalPosition = ONBtnTransform.localPosition;
        OFFBtnOriginalPosition = OFFBtnTransform.localPosition;
    }

    private void Start()
    {
        // initialize button positions based on IsOn state
        if (IsOn)
        {
            OnTurnedOn();
        }
        else
        {
            OnTurnedOff();
        }
    }
    public void OnMouseDown() 
    {
    
        if (!IsActive) return;

        IsOn = !IsOn;

        if (IsOn)
        {
            OnTurnedOn();
        }
        else
        {
            OnTurnedOff();
        }
    }
    public void OnTurnedOn()
    {
        colorChanger.ChangeGlobalMaterialColor();

        ONBtnTransform.localPosition = ONBtnOriginalPosition + pressedOffset; // pressed
        OFFBtnTransform.localPosition = OFFBtnOriginalPosition;
    }
    public void OnTurnedOff()
    {
        colorChanger.ResetMaterialColor();
        ONBtnTransform.localPosition = ONBtnOriginalPosition;
        OFFBtnTransform.localPosition = OFFBtnOriginalPosition + pressedOffset; // pressed
    }

}