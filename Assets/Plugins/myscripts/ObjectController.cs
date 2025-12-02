using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Vienna Ly
 * Allows mouse control over the object. 
 * Place this script on Main Camera - or main object with cam controller and activity controller.  
 *  -Script will control vertical rotation of g.o. and horizontal rotation of target.
 *  -If this is applied with KGF camera, it will turn off the KGF rotation control of the camera.
 */


public class ObjectController : MonoBehaviour {

	[Tooltip("modifies horizontal rotation speed")]
	public float horizontalSpeed = 2.0F;
	[Tooltip("modifies vertical rotation speed")]
	public float verticalSpeed = 2.0F;
	[Tooltip("set transform target -- default is first child of this game object")]
	public Transform target;
	public bool invert_hAxis = false;
	public bool invert_vAxis = false;

	private GameObject holder;  


	void Start(){
		// disable KGF Cam rotation controls if present (object rotation over-rides cam orbit)
		if (gameObject.GetComponent<KGFOrbitCam> () != null) {
			KGFOrbitCam camController = gameObject.GetComponent<KGFOrbitCam> ();
			camController.SetRotationEnable (false);
		}

		// create holder
		holder = new GameObject("__holder__");

		// if no new target is assigned, use activity controller target
		if (target == null && gameObject.GetComponent<ActivityController> () != null) {
			ActivityController ac = gameObject.GetComponent<ActivityController> ();
			target = ac._MODEL.transform;
		} else if (target == null) {
			// if no activity controller then target should be first child of this holder game object
			target = GetComponentInChildren<Transform>();  Debug.Log(target);
		}

		target.parent = holder.transform;
	}

	void Update() {
		int axis_adjust_h = (invert_hAxis)? -1 : 1;  // multiplier for h-axis, if needs to be adjusted
		int axis_adjust_v = (invert_vAxis)? -1 : 1;  // multiplier for v-axis, if needs to be adjusted
		if (Input.GetMouseButton (1)) {
			float h = axis_adjust_h * horizontalSpeed * Input.GetAxis("Mouse X");
			float v = axis_adjust_v * verticalSpeed * Input.GetAxis("Mouse Y");

			// adjust for when model rotated upside down, axis flip
			if (holder.transform.eulerAngles.x >= 90 && holder.transform.eulerAngles.x < 270 || 
				holder.transform.eulerAngles.z > 0 ||
				holder.transform.eulerAngles.y > 0) {
				target.Rotate(0, -h, 0);	
			} else {
				target.Rotate(0, h, 0);
			}

			holder.transform.Rotate (v, 0, 0);
		}

	}
}

