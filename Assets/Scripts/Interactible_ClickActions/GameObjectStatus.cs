using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary>Add to object to allow expose control of active status target game object (go)</summary>*/

[System.Serializable]
public class CA_GameObjectsStatus : ClickAction
{
    [Header("Game Object Status Settings")]
    [Tooltip("Game Object to set active/inactive")]
    public GameObject targetGameObject;

    void Start()
    {
        if (isOn) targetGameObject.SetActive(true);
        else targetGameObject.SetActive(false);
    }
    public override void OnClick()
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
}