/// <summary>
/// Abstract class for switch button that toggles between on and off states
/// On and Off button game objects are depressed/raised based on the state and 
/// toggles IsOn property
/// </summary>
using UnityEngine;
using UnityEngine.EventSystems;

public class SwitchButtonsToggle : ClickAction, ISwitch
{
    [Header("Switch Button Settings")]
    [Tooltip("transform of the ON geometry to animate")]
    [SerializeField] protected Transform ONBtnTransform;
    [Tooltip("transform of the OFF geometry to animate")]
    [SerializeField] protected Transform OFFBtnTransform;
    [Tooltip("Offset to apply to the button when pressed")]
    [SerializeField] protected Vector3 pressedOffset = new Vector3(0f, 0f, -1.1f);
        
    
    // for button press animation
    private Vector3 ONBtnOriginalPosition; 
    private Vector3 OFFBtnOriginalPosition;
    
    public bool IsOn { get => isOn; set => isOn = value; }
    public bool IsActive { get; set; } = true;
    protected virtual void Start()
    {
        ONBtnOriginalPosition = ONBtnTransform.localPosition;
        OFFBtnOriginalPosition = OFFBtnTransform.localPosition;

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
    public override void OnClick()
    {
        if (!IsActive) return;

        if (onClickActiveMode == GameObjectActiveMode.SetTrue)
        {
            IsOn = true;
        }
        else if (onClickActiveMode == GameObjectActiveMode.SetFalse)
        {
            IsOn = false;
        }
        else if (onClickActiveMode == GameObjectActiveMode.Toggle)
        {
            IsOn = !IsOn;
        }

        if (IsOn)
        {
            OnTurnedOn();
        }
        else
        {
            OnTurnedOff();
        }
    }
    public virtual void OnTurnedOn()
    {
        ONBtnTransform.localPosition = ONBtnOriginalPosition + pressedOffset; // pressed
        OFFBtnTransform.localPosition = OFFBtnOriginalPosition;
    }
    public virtual void OnTurnedOff()
    {
        ONBtnTransform.localPosition = ONBtnOriginalPosition;
        OFFBtnTransform.localPosition = OFFBtnOriginalPosition + pressedOffset; // pressed
    }
}