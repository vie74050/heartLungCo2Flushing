using UnityEngine;

public enum GameObjectActiveMode
{
    SetTrue,
    SetFalse,
    Toggle
}
public abstract class ClickAction : MonoBehaviour {
    [Tooltip("What to do on click")]
    public GameObjectActiveMode onClickActiveMode;
    [Tooltip("Initial state of the switch button")]
    public bool isOn = false;
    
    public abstract void OnClick();
}