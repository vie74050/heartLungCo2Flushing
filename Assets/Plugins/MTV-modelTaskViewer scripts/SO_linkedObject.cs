﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Helper to SelectableObject -- selects additional objects specified
 * in linkedObjects
 * */
// place on item with a SelectableObject

public class SO_linkedObject : SelectableObject {
	
	public GameObject[] linkedObjects;

	private List<GameObject> linkedObjectsList;

	public override void Init()
	{
		base.Init();
		SelectableObject this_so = transform.GetComponent<SelectableObject>();

		// check that this GO is not part of linkedObjects
		linkedObjectsList = new List<GameObject>();
		foreach (GameObject go in linkedObjects)
		{
			if (go != gameObject)
			{
				linkedObjectsList.Add(go);
				//print(go.name);
			}
			
		}

		foreach (GameObject go in linkedObjectsList)
		{
			//print(go.name);
			SelectableObject so = go.GetComponent<SelectableObject>();
			if (so != null)
			{
				// make all linked objects behave as one under this GO
				// i.e appear as single item in list, all gets selected and deselected together
				so.isDraggable = false;
				so.isSelected = false;
				so.isListItem = false;
				//print(so.transform.name + " 2: " + so.isListItem);
			}
			else
			{
				go.SetActive(this_so.isSelected);
			}
			
		}
	}
	public override void Select(){
		//print(linkedObjects);
		base.Select();
		
		// also select linkedObjects
		foreach (GameObject go in linkedObjectsList) {
			SelectableObject so = go.GetComponent<SelectableObject>();
			//print("link object select: " + go.name);
			if (so !=null)
			{
				so.alphaMode = alphaMode;
				so.isDraggable = false;
				so.isSelected = false;
				so.Select();
			}
			else
			{
				go.SetActive(true);
			}
		}

	}

	public override void Deselect(){
		base.Deselect();
		// also deselects linkedObjects
		if (linkedObjectsList.Count>0) {
			foreach (GameObject go in linkedObjectsList) {
				SelectableObject so = go.GetComponent<SelectableObject>();
				
				if (so != null)
				{
					so.alphaMode = alphaMode;
					so.isDraggable = false;
					so.isSelected = false;
					so.Deselect();
					
				}
				else
				{
					go.SetActive(false);
				}
			}
		}
		
		

	}
}