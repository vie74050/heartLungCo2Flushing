using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Makes Selectable Object droppable on collision with specified dropzone collider
 *  - inherits from SelectableObject
 *  - on mouse drag, checks for dropzone collision
 *  - on mouse up, if over dropzone, snaps to target with the same name that is set in the Dropzone checklist
 *  - can reparent to target parent on drop
 *  - can mask child colliders until dropped
 * NB: target objects in dropzone checklist are only used for position/rotation reference and are hidden on start.
 */

public class SO_Dropable : SelectableObject
{
	[Tooltip("Move w parent after dropped?")]
	public bool reparentOnDrop = false;

	[Tooltip("Turn off child colliders until dropped on target dropzone")]
	public bool maskChildUntilDropped = false;

	[Tooltip("Remains draggable after drop?")]
	public bool isDraggableAfterDrop = false;
	
	[Tooltip("Default dist from camera if not over dropzone")]
	public float distFromCamDefault = 5;

	private readonly string dzLayerName = "Ignore Raycast";
	private LayerMask dropzoneMask;
	private float hitdistance = 1;
	private Transform startParent;

	private void Start()
	{
		base.Start(); // Call Start from SelectableObject

		startParent = transform.parent;

		// nb: assign dropzone items to Ignore Raycast layer
		dropzoneMask = LayerMask.GetMask(dzLayerName);

		if (maskChildUntilDropped)
		{
			SetChildrenColliders(false);
		}
	}
	private void OnMouseDrag()
	{
		base.OnMouseDrag(); // Call OnMouseDrag from SelectableObject
		SetDzsLayer("Default");
		bool isOver = IsOverDropzone();

		if (isOver)
		{
			distFromCam = hitdistance;
		}
		else
		{
			distFromCam = distFromCamDefault < screenPoint.z ? distFromCamDefault : screenPoint.z;
		}
	}
	private void OnMouseUp()
	{
		base.OnMouseUp(); // Call OnMouseUp from SelectableObject
		if (isDraggable && !alphaMode)
		{
			OnDropzone(GetOverDropzone());
			Deselect();
		}
		
		SetDzsLayer(dzLayerName);
	}
	private void OnDropzone(Dropzone dz)
	{
		// handle object being dropped on a dropzone
		//print("ondropzone");

		Transform target = transform;
		// check for specific dropzone target
		if (dz != null)
		{
			if (dz.IsAllowed(name))
			{
				target = dz.GetTransformFromList(transform);
				
				if (target != transform)
				{
					Drop(target);
				}
			}
			else
			{
				ResetPosition();
			}
			
		}
		
	}
	private void Drop(Transform target)
	{
		// drop action 
		print("drop " + target.name);

		// snap to target pos and rot
		transform.position = target.position;
		transform.eulerAngles = target.eulerAngles;
		
		if (reparentOnDrop)
		{
			// attach to parent (reparaent so object now moves w target parent)
			transform.parent = target.parent;
		}

		// turn on children colliders, only if this is no longer draggable
		// to avoid conflict
		if (maskChildUntilDropped)
		{
			SetChildrenColliders(true);
		}

		if (!isDraggableAfterDrop){
			isDraggable = false;        
		}

	}

	/// <summary>
	/// Returns true if mouse is over Dropzone
	/// </summary>
	/// <returns></returns>
	private bool IsOverDropzone()
	{
		bool isOver = false;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Color ray_color = Color.white;
		
		if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, dropzoneMask))
		{
			Transform t = hit.transform;
			Dropzone dz = t.GetComponent<Dropzone>();

			if (dz)
			{
				if (dz.IsAllowed(name))
				{
					ray_color = Color.green;
				}
				isOver = true;

				hitdistance = Camera.main.transform.position.z - hit.point.z; //hit.distance; 
				//Debug.Log(hitdistance);
			}

			Debug.DrawRay(ray.origin, ray.direction * hit.distance, ray_color);
			//print(name + " on " + hit.transform.name + " : " + allowed);
					
		}
			
		return isOver;
	}

	/// <summary>
	/// Return Dropzone if mouse is over a dropzone.
	/// </summary>
	/// <returns></returns>
	private Dropzone GetOverDropzone()
	{
		// detect if pointer collision over dropzone
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Dropzone dz = null;

		if (Physics.Raycast(ray, out hit, Mathf.Infinity, dropzoneMask))
		{

			Transform t = hit.transform;
			dz = t.GetComponent<Dropzone>();
			
			Debug.Log("Mouse over collider " + t.name);
		}

		return dz;
	}
	
	/// <summary>
	/// Set all child colliders.enabled to setting to turn all child coliders on or off
	/// </summary>
	/// <param name="setting"></param>
	private void SetChildrenColliders(bool setting)
	{
		Collider[] _cols = GetComponentsInChildren<Collider>();
		for (var i = 1; i < _cols.Length; i++)
		{
			_cols[i].enabled = setting;
		}
		//print("collider " + name + ", " +  _cols.Length);
	}

	/// <summary>
	/// Sets all children Dropzone layer to targetlayer
	/// </summary>
	/// <param name="targetlayer">Name of target layer</param>
	private void SetDzsLayer(string targetlayer)
	{
		Dropzone[] dzs = GetComponentsInChildren<Dropzone>();
		int i_layer = LayerMask.NameToLayer(targetlayer);
		foreach (Dropzone _dz in dzs)
		{
			_dz.gameObject.layer = i_layer;
		}
		
	}

	/// <summary>
	/// Returns the name of the current parent object, to check where object is dropped
	/// </summary>
	public string GetCurrentParentName()
	{
		return transform.parent.name;
	}
	public void ResetPosition()
	{
		if (reparentOnDrop)
		{
			// reparent to original parent
			transform.parent = startParent;
		}
		// reset to start position
		base.ResetAll();
	}
}
