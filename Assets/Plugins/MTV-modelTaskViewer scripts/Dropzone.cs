using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/** Make collider behave as a drop zone, set dropable requirement
 * - children G.Os define set target positions for dropables (based on name)
 * - check to see if colliding object can be dropped
 * - check to see if list complete
 */

[RequireComponent(typeof(Collider))]
public class Dropzone : MonoBehaviour
{
	//string[] alllowedSOList;
	[Tooltip("Reference Transforms for target drop positions. Targets matched by g.o. name")]
	public List<DropzoneList> checklist;

	[Tooltip("Set the highlight tint")]
	public Color highlightColour;

	[Tooltip("Set inactive when complete")]
	public bool hideOnComplete = true;

	private Material[] origMaterials;
	private Material mat_transparent;
	// references
	private LayerMask dropzoneMask;
	private void Start()
	{
		
		// nb: assign dropzone items to Ignore Raycast layer
		dropzoneMask = LayerMask.GetMask("Ignore Raycast");

		if (checklist.Count == 0)
		{
			PopulateListWithChildren();
		}

		foreach(DropzoneList li in checklist)
		{
			// hide checklist items bc they're only used for transform reference
			li.tn.gameObject.SetActive(false);
			
		}

		// make sure transparent material is in the Resources/materials and shaders folder
		mat_transparent = Resources.Load("materials and shaders/transparent") as Material;
		
		// save orig materials
		SetOrigMaterials();		
	}

	private void Update()
	{
		
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Color ray_color = Color.white;
		bool isHit = false;
		if (Physics.Raycast(ray, out hit, Mathf.Infinity, dropzoneMask))
		{
			isHit = (hit.transform == transform);	
		}

		if (isHit && Input.GetMouseButton(0))
		{
			ChangeMaterialColor("select");
		}
		else
		{
			ChangeMaterialColor("deselect");
		}

		
	}

	/// <summary>
	/// Checks if s is in checklist and not already checked off
	/// </summary>
	/// <param name="s">name of game object to check</param>
	/// <returns></returns>
	public bool IsAllowed(string s)
	{
		// if dropzone doesn't require any drop items, disallow all
		bool allowed = false;
		// check if s is in allowedSOList
		for (int i = 0; i < checklist.Count; i++)
		{
			DropzoneList ls_item = checklist[i];
			string listname = ls_item.GetName();
			string objname = s.Trim();
			int compare = listname.CompareTo(objname);
			//print(listname + ", " + objname + " is " + compare);
			if (compare==0 && !ls_item.isComplete)
			{
				allowed = true;
				break; 
			}
		}

		//print(s + " on " + name + " allowed: " + allowed);
		return allowed;
	}
	
	/// <summary>
	/// Returns first transform with matching name as t that is not checked off list 
	/// If no match, returns original transform t
	/// </summary>
	/// <param name="t">Transform to check</param>
	/// <returns></returns>
	public Transform GetTransformFromList(Transform t)
	{
		string s = t.name.Trim();
		Transform return_t = t;
		for (int i = 0; i < checklist.Count; i++)
		{
			DropzoneList ls_item = checklist[i];
			string listname = ls_item.GetName();
			int compare = listname.CompareTo(s);
			//print(listname + ", " + s + " is " + compare);
			if ( compare==0 && !ls_item.isComplete)
			{
				return_t = ls_item.tn;
				
				CrossOffList(ls_item);
				break;
			}
		}
		
		return return_t;
	}

	/// <summary>
	/// Creates checklist from first children of this game object
	/// </summary>
	private void PopulateListWithChildren()
	{
		Transform[] tns = gameObject.GetComponentsInChildren<Transform>();

		checklist = new List<DropzoneList>();
		//print("**DROPZONE** " + name);
		foreach (Transform tn in tns)
		{
			if (tn != transform && tn.parent == transform)
			{
				DropzoneList dz_item = new DropzoneList(tn);
				checklist.Add(dz_item);
				
				//print(dz_item.name);
			}

		}
	}
	/** check if all items on list checked off (i.e. complete)
	 */
	private void CrossOffList(DropzoneList item)
	{
		item.isComplete = true;
		//print(item.GetName() + " dropped on " + name);

		if (IsComplete())
		{
			// handle complete
			CompleteHandler();
		}
	}
	private void CompleteHandler()
	{
		//gameObject.SetActive(false);
		//print(name + " DROPZONE is complete");
		if (hideOnComplete)
		{
			gameObject.SetActive(false);
		}

	}
	/** check if all items on list checked off (i.e. complete)
	 */
	private bool IsComplete()
	{
		bool iscomplete = true;
		for (int i = 0; i < checklist.Count; i++)
		{
			DropzoneList ls_item = checklist[i];
			if (!ls_item.isComplete)
			{
				iscomplete = false;
				break;
			}
		}
		return iscomplete;
	}

	private void SetOrigMaterials()
	{

		if (GetComponent<Renderer>())
		{
			int i = 0;
			origMaterials = GetComponent<Renderer>().materials;
			foreach (Material m in GetComponent<Renderer>().materials)
			{   // store object's original materials
				// enable modification of Standard Shader
				// see docs : https://docs.unity3d.com/Manual/MaterialsAccessingViaScript.html
				m.EnableKeyword("_ALPHABLEND_ON");
				m.EnableKeyword("_EMISSION");
				m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
				m.EnableKeyword("_Color");
				m.EnableKeyword("_Mode");
				origMaterials[i] = new Material(GetComponent<Renderer>().materials[i]);

				i++;
			}
		}
	}
	private void ChangeMaterialColor(string state)
	{
		// using standard shader 

		GameObject obj = gameObject;

		Color selectedColor = highlightColour;

		Renderer objRenderer = obj.GetComponent<Renderer>();

		int mat_i = 0;

		if (objRenderer != null && origMaterials != null)
		{
			foreach (Material m in objRenderer.materials)
			{

				Material origMat = origMaterials[mat_i];
				Color c = origMat.color;
				Color c_emit = Color.black * Mathf.LinearToGammaSpace(0.9f);

				switch (state)
				{

					case "select":

						// intensity of selectedColor determined by selectedColor alpha 

						Color emit = Color.Lerp(origMat.color, selectedColor, selectedColor.a);
						c_emit = emit * Mathf.LinearToGammaSpace(selectedColor.a);
						c = emit;// origMat.color;

						break;

					case "transparent":

						m.shader = mat_transparent.shader;
						c.a = (mat_transparent.color.a);
						StandardShaderUtils.ChangeRenderMode(m, StandardShaderUtils.BlendMode.Fade);

						break;

					default:

						c = origMat.color;
						m.shader = origMat.shader;

						StandardShaderUtils.ChangeRenderMode(m, (StandardShaderUtils.BlendMode)origMat.GetFloat("_Mode"));

						break;
				}

				m.SetColor("_Color", c);
				m.SetColor("_EmissionColor", c_emit);
				m.color = c;
				mat_i++;

			}
		}


	}
}

[System.Serializable]
public class DropzoneList
{
	public bool isComplete = false;
	
	public Transform tn;
	Vector3 pos;
	Vector3 rot;

	public DropzoneList(Transform t)
	{
		
		tn = t;
		pos = t.position;
		rot = t.eulerAngles;	
	}
	public string GetName()
	{
		//Debug.Log(tn.name);
		return tn.name.Trim();
	}
}