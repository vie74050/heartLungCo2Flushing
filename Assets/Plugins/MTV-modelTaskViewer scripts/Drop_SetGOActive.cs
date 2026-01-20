/*
* Handles setting specified GameObjects active when a drop event occurs.
* Attach this script to a GameObject that should respond to drop actions.
* 
* How to implement:
* Attach this script to a GameObject in your Unity scene.
* Populate the <c>GOsToSetOnDrop</c> list in the Inspector with <c>GameObjectsStatus</c> references you want to activate on drop.
* Ensure the object implements the <c>IDropHandler</c> interface and that drop events are properly routed to this script's <c>eDrop</c> method.
*/

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drop_SetGOActive : MonoBehaviour, IDropHandler
{
    [Tooltip("GOs to set active on drop")]
    public List<GameObjectsStatus> GOsToSetOnDrop;

    public void Init()
    {
        
    }
    public void eDrop(Transform target)
    {

        if (GOsToSetOnDrop.Count > 0)
        {
            foreach (GameObjectsStatus _gos in GOsToSetOnDrop)
            {
                _gos.SetActiveStatus();
            }
        }
    }

}