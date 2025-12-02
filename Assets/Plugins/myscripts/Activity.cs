using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Activity : MonoBehaviour {
	
	[Tooltip("Optional: Show or hide activity")]
	public bool showActivity = false;

	[Tooltip("show or hide task steps")]
	public bool showActivitySteps = false;

	[Tooltip("show or hide log button")]
	public bool showLogBtn = false;


	[HideInInspector]
	public GameObject[] activityList;

	[HideInInspector]
	public List<EditorGameObject> taskItems;

	[Tooltip("Short sentance describing task.")]
	public string activityTaskName;

}
