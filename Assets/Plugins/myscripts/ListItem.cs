using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class ListItem {
	public int id ;
	public bool showBtn;
	public bool selected;
	//public bool tog_open;
	public GameObject node;
	public int parent_i;
	public List<int> childIndices;
	public SelectableObject so;
	public string objPathName;

	public ListItem (int index, int pi, GameObject go){
		node = go;
		showBtn = (pi==0);
		//tog_open = index==0;
		id = index;
		parent_i = pi;
		childIndices = new List<int>();

		so = node.GetComponent<SelectableObject>();

		selected = false;
		objPathName = GetGameObjectPath(go);
	}	

	public void AddChildRef(int i) {

		childIndices.Add(i);

	}

	private static string GetGameObjectPath(GameObject obj)
	{
		string path =  obj.name;
		while (obj.transform.parent != null)
		{
			obj = obj.transform.parent.gameObject;

			if (obj.transform.parent != null)
				path = obj.name + "/" + path;
		}
		return path;
	}

}

public class ListChecklistItem{
	public GameObject go;
	public GameObject targetGO;  // reference to _target object w same name
	public string name;

	public bool complete;
	public Vector3 targetPos;
	public Vector3 origPos;

	private bool moving = false;
	public ListChecklistItem (GameObject _go, GameObject _TARGET){
		// NB: reserve use of '_' to distinguish multiple objects with same name
		//Char delimiter = '_';
		name = _go.name;

		go = _go;
		complete = false;
		targetPos = go.transform.localPosition;
		origPos = go.transform.localPosition;

		if (_TARGET != null) {
			Transform t = _TARGET.transform.Find (name);
			if (t != null) {
				targetGO = t.gameObject;

				targetPos = targetGO.transform.position;

			} 
		}
			
	}

}