using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary>Add to object to allow expose control of active status target game object (go)</summary>*/
public enum GameObjectActiveMode
{
    SetTrue,
    SetFalse,
    Toggle
}

[System.Serializable]
public class CA_GameObjectsStatus : ClickAction
{
    [Tooltip("Game Object to set active/inactive")]
    public GameObject targetGameObject;

    [Tooltip("Active mode to set")]
    public GameObjectActiveMode onClickActiveMode;

    private void SetActiveStatus()
    {
        switch (onClickActiveMode)
        {
            case GameObjectActiveMode.SetTrue:
                targetGameObject.SetActive(true);
                break;
            case GameObjectActiveMode.SetFalse:
                targetGameObject.SetActive(false);
                break;
            case GameObjectActiveMode.Toggle:
                targetGameObject.SetActive(!targetGameObject.activeSelf);
                break;
        }
    }

    public override void OnClick()
    {
        SetActiveStatus();
    }
}