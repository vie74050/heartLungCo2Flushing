using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// place on item with a SelectableObject

public class SO_linkedObject : MonoBehaviour {
	
	public GameObject[] linkedObjects;

	private void Start(){
		

		foreach (GameObject go in linkedObjects) {
			go.SetActive (false);
		}
	}

	private void e_selected(){

		// listens for selected event from SelectableObject on self or parent


		foreach (GameObject go in linkedObjects) {
			go.SetActive (true);
		}

	}

	private void e_deselected(){

		// listens for selected event from SelectableObject on self or parent


		foreach (GameObject go in linkedObjects) {
			go.SetActive (false);
		}

	}
}
