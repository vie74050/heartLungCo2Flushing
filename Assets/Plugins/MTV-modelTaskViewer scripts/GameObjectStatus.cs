using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**<summary>Add to object to allow expose control of active status or colliders of another</summary>*/
[System.Serializable]
public class GameObjectsStatus : MonoBehaviour
{
    [Tooltip("Game Object to set active/inactive")]
    public GameObject go;
    [Tooltip("Active setting to set")]
    public bool status;

    public void SetActiveStatus()
    {
        go.SetActive(status);
       
    }
    public void ColliderEnable()
    {
        go.GetComponent<Collider>().enabled = status;

    }
}