using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Makes Selectable Object droppable on collision with specified dropzone collider
 *  - require Selectable Object
 */

public class Dropable : MonoBehaviour
{
	[Tooltip("Move w parent after dropped?")]
	public bool reparentOnDrop = false;

	[Tooltip("Turn off child colliders until dropped on target dropzone")]
	public bool maskChildUntilDropped = false;

	[Tooltip("Remains draggable after drop?")]
	public bool isDraggableAfterDrop = false;
	
	[Tooltip("Default dist from camera if not over dropzone")]
	public float distFromCamDefault = 5;

	private SelectableObject so;
	private readonly string dzLayerName = "Ignore Raycast";
	private LayerMask dropzoneMask;
	private float hitdistance = 1;

	private void Start()
	{
		// nb: assign dropzone items to Ignore Raycast layer
		dropzoneMask = LayerMask.GetMask(dzLayerName);

		so = GetComponent<SelectableObject>();

		if (maskChildUntilDropped)
		{
			SetChildrenColliders(false);
		}
	}
	private void OnMouseDrag()
	{
		SetDzsLayer("Default");
		bool isOver = IsOverDropzone();

		if (isOver)
		{
			so.distFromCam = hitdistance * .9f;
			//so.snapbackDistMin = 100;
		}
		else
		{
			so.distFromCam = distFromCamDefault < so.screenPoint.z ? distFromCamDefault : so.screenPoint.z;
			//so.snapbackDistMin = 2;
		}
	}
	private void OnMouseUp()
	{
		if (so.isDraggable && !so.alphaMode)
		{
			OnDropzone(GetOverDropzone());
			so.Deselect();
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
				so.ResetAll();
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
			so.isDraggable = false;		
		}

		so.isTaskComplete = true;

		IDropHandler[] idhs = GetComponents<IDropHandler>();
		foreach(IDropHandler idh in idhs)
		{
			if (idh != null)
			{
				idh.eDrop(target);
			}
		}
		
		//BroadcastMessage("eDrop", target,SendMessageOptions.DontRequireReceiver);
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

				hitdistance = Camera.main.transform.position.y - hit.point.y; //hit.distance; 
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
}