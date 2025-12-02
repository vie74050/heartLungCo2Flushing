using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

[CustomEditor(typeof(ActivityController))]

public class ActivityInspector : Editor {
	private ReorderableList reorderableList;

	private ActivityController activity
	{
		get
		{
			return target as ActivityController;
		}


	}

	private void OnEnable()
	{
		reorderableList = new ReorderableList(activity.taskItems,typeof(EditorGameObject), true, true, true, true);

		
		// Add listeners to draw events
		reorderableList.drawHeaderCallback += DrawHeader;
		reorderableList.drawElementCallback += DrawElement;

		reorderableList.onAddCallback += AddItem;
		reorderableList.onRemoveCallback += RemoveItem;
        reorderableList.onReorderCallback += ReorderItem;
	}

	private void OnDisable()
	{
		// Make sure we don't get memory leaks etc.
		reorderableList.drawHeaderCallback -= DrawHeader;
		reorderableList.drawElementCallback -= DrawElement;

		reorderableList.onAddCallback -= AddItem;
		reorderableList.onRemoveCallback -= RemoveItem;
        reorderableList.onReorderCallback -= ReorderItem;
	}

	/// <summary>
	/// Draws the header of the list
	/// </summary>
	/// <param name="rect"></param>
	private void DrawHeader(Rect rect)
	{
		EditorGUI.BeginChangeCheck();
		bool listEmpty = true;

		// check that at least one game object has been set
		foreach (EditorGameObject item in activity.taskItems) {
			if (item.goRef != null) {
				
				listEmpty = false;
				break;
			}
		}
		//Debug.Log ("listempty: " + listEmpty);
		// create optional parameters in inspector UI elements
		GUI.Label(rect, "Task Step List");

		if (activity.taskItems.Count > 0 && (!listEmpty)) {
			
			activity.activityTaskName = EditorGUILayout.TextField ("Task Name: ", activity.activityTaskName);
			activity.showActivity = GUILayout.Toggle (activity.showActivity, "Show Task");


			if (activity.showActivity) {
                activity.TargetEndPos = EditorGUILayout.ObjectField(activity.TargetEndPos, typeof(GameObject), true) as GameObject;
				activity.showActivitySteps = GUILayout.Toggle (activity.showActivitySteps, "Show Steps");
				activity.showLogBtn = GUILayout.Toggle (activity.showLogBtn, "Show Log Button");
			}

		} 

		if (listEmpty) {
			activity.showActivity = false;
		}

		if (EditorGUI.EndChangeCheck ()) {
			EditorUtility.SetDirty(target);
		}

	}

	/// <summary>
	/// Draws one element of the list (EditorGameObjec)
	/// </summary>
	/// <param name="rect"></param>
	/// <param name="index"></param>
	/// <param name="active"></param>
	/// <param name="focused"></param>
	private void DrawElement(Rect rect, int index, bool active, bool focused)
	{
		EditorGameObject item = activity.taskItems[index];

		EditorGUI.BeginChangeCheck();
		//item.selectedItem = EditorGUI.Toggle(new Rect(rect.x, rect.y, 18, rect.height), item.selectedItem);
		item.goRef = (GameObject)EditorGUI.ObjectField(new Rect(rect.x + 18, rect.y, rect.width - 18, rect.height), "", item.goRef, typeof(GameObject), true);
		if (EditorGUI.EndChangeCheck())
		{
			EditorUtility.SetDirty(target);
		}

	}

	private void AddItem(ReorderableList list)
	{
		activity.taskItems.Add(new EditorGameObject());

		EditorUtility.SetDirty(target);
	}

	private void RemoveItem(ReorderableList list)
	{
		
		activity.taskItems.RemoveAt (list.index);

		EditorUtility.SetDirty(target);
	}

    private void ReorderItem(ReorderableList list){
        // update taskItems on reorder
       
        activity.taskItems = reorderableList.list as List<EditorGameObject>;

        EditorUtility.SetDirty(target);
        
    }
	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		// Actually draw the list in the inspector
		reorderableList.DoLayoutList();

	}

}
