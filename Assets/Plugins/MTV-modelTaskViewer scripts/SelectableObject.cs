using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/* multi-select ability by pressing SHIFT -- adds the tag Draggable to objects 

NB: MATERIALS should use STANDARD shaders.  Do not use Fade mode for transparent items, use Transparent mode instead.
Standard shader Fade mode reserved for setting visibility of non-selected parts

	Default - will add mesh collider if no collider available.  
	
	To add drag constraints - add rigidbody constraints.
	By default positional changes transforms object's position unless rigidbody is preset
*/

public class SelectableObject : MonoBehaviour
{

	[DllImport("__Internal")]
	private static extern void BrowserSelect(string str);

	[Tooltip("Optional display name instead of transform name")]
	public string displayName = "";

	[Tooltip("if true, object is highlighted ")]
	public bool isSelected = false;

	[Tooltip("if true, object is transparent when deselected")]
	public bool alphaMode = false;

	[Tooltip("Set the highlight tint")]
	public Color highlightColour;

	[Tooltip("Whether or not to set to snapToTransform if selected")]
	public bool doSnapToTarget = false;
	[Tooltip("Assign transform to snap to after selected")]
	public Transform snapToTransform;

	// draggable vars
	public float snapbackDistMin = 5;
	public float snapbackDistMax = 15;
	
	public bool isDraggable = true;                                 // set to make it draggable 
	public bool isListItem = true;

	[HideInInspector]
	public Vector3 targetPos = new Vector3(0, 10000, 0);            // local position ref
	public bool isTaskComplete = false;

	//private Transform pivotObj; 									// adjust for wrong pivot points
	private Material[] origMaterials;

	private Material mat_transparent;

	public Vector3 screenPoint;

	private Vector3 offset;
	private Vector3 origPos;                                        // position for reset
	private Vector3 origRot;                                        // rot for reset
	private float origSnapbackDistMin = 5;
	private float origSnapbackDistMax = 15;
	private animationOverride ao;

	private readonly string tag_drag = "Draggable";

	private KGFOrbitCam camsettings;
	private Vector3 origCamTargetPos;
	public float distFromCam = 5;

	protected void Awake () {
		if (displayName == "") {
			displayName = gameObject.name;
		}
	}
	protected void Start()
	{
		Init();
	}
	public virtual void Init()
	{

		camsettings = Camera.main.GetComponent<KGFOrbitCam>();
		GameObject camTarget = camsettings.GetTarget();
		origCamTargetPos = new Vector3(camTarget.transform.position.x, camTarget.transform.position.y, camTarget.transform.position.z);

		// make sure transparent material is in the Resources/materials and shaders folder
		mat_transparent = Resources.Load("materials and shaders/transparent") as Material;

		// save orig materials
		SetOrigMaterials();

		// adjusted pivot ref if any
		//Transform[] pivot = GetComponentsInChildren<Transform>();
		//pivotObj = pivot[pivot.Length-1];

		// draggable: save orig positions
		origPos = transform.localPosition;
		origRot = transform.localEulerAngles;
		origSnapbackDistMin = snapbackDistMin;
		origSnapbackDistMax = snapbackDistMax;

		if (alphaMode)
		{
			ChangeMaterialColor("transparent");
		}
		if (isDraggable)
		{
			// add convex mesh collider if not already has collider
			if (GetComponent<Collider>() == null)
			{
				gameObject.AddComponent<MeshCollider>();
				MeshCollider mc = gameObject.GetComponent<MeshCollider>();
				mc.convex = true;
			}
		}
		
		if (gameObject.GetComponent<animationOverride>())
		{
			ao = gameObject.GetComponent<animationOverride>();
		}
		else
		{
			ao = gameObject.AddComponent<animationOverride>();
			ao.animOverride = false;
		}

		if (snapToTransform != null && doSnapToTarget) {
			targetPos = snapToTransform.position;
			// TODO rotation too?
		}

	}

	void Update()
	{
		float speed = 10f;
		if (doSnapToTarget && !ao.animOverride && isTaskComplete)
		{

			transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

		}

	}

	public void SetTargetPos(Transform tar)
	{

		//target.parent = transform.parent;
		targetPos = tar.localPosition;
		//Destroy(target.gameObject);

	}
	public void TriggerAnim()
	{

		if (ao)
		{
			ao.PlayAnim();
		}
	}
	public Material[] GetOriginalMaterials()
	{
		return origMaterials;
	}
	//public float GetOrigScreenPointZ()
	//{
	//	float origScreenPoint_z;
	//	Vector3 sp = Camera.main.WorldToScreenPoint(origPos);
	//	origScreenPoint_z = sp.z;
	//	return origScreenPoint_z;
	//}
	public virtual void Deselect()
	{
		
		if (alphaMode)
			ChangeMaterialColor("transparent");
		else
			ChangeMaterialColor("deselect");

		gameObject.tag = "Untagged";

		isSelected = false;

		//Debug.Log ("changed - deselect");
		gameObject.BroadcastMessage("e_deselected", SendMessageOptions.DontRequireReceiver);

	}

	public virtual void Select()
	{
				
		//Debug.Log ("changed - select" + transform.name);
		ChangeMaterialColor("select");
		isSelected = true;

		if (isDraggable) {
			gameObject.tag = tag_drag;
		}

		// broadcast to web container -- as of Unity 5.6 legacy way
		if (Application.platform == RuntimePlatform.WebGLPlayer && isListItem)
		{
			
			#if UNITY_WEBGL && !UNITY_EDITOR
				BrowserSelect(transform.name);
			
			#endif

		}
		
		if (doSnapToTarget && snapToTransform !=null) {
			isTaskComplete = true;
		}

		gameObject.BroadcastMessage("e_selected", SendMessageOptions.DontRequireReceiver);
		
	}
	public virtual void ResetAll()
	{
		snapbackDistMin = origSnapbackDistMin;
		snapbackDistMax = origSnapbackDistMax;
		ResetPosition();
		ResetRotation();
		Deselect();
	}
	public virtual void ResetPosition()
	{
		transform.localPosition = origPos;
		doSnapToTarget = false;
	}

	public virtual void ResetRotation()
	{
		transform.localEulerAngles = origRot;
	}

	/// <summary>
	/// If this selectable object is a task step, query to see if complete.
	/// </summary>
	/// <returns>bool: if this step is considered complete. </returns>
	public virtual bool IsTaskComplete()
	{
		
		// override to set complete logic
		return isTaskComplete;
	}
	protected void OnMouseDown()
	{
		if (!alphaMode)
		{
			GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);

					if (isSelected &&
						(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) )
					{

						Deselect();
					}
					else
					{
						

						// if not SHIFT -- deselect all others and start new selection
						if (isDraggable && !(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
							&& gameObject.tag != tag_drag)
						{
							foreach (GameObject go in dragTagObjs)
							{
								SelectableObject so = go.GetComponent<SelectableObject>();
								if (so != null)
									so.Deselect();

							}
						}

						Select();

					}

					// save positions for drag reference
					dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);
					foreach (GameObject go in dragTagObjs)
					{
						SelectableObject md = go.GetComponent<SelectableObject>();
						if (md != null)
						{
							md.screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
							md.offset = go.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, md.screenPoint.z));
						}
					}

					if (isDraggable) 
					{ 
						SetFreezeRigidBody(false);
					}
		}
		
	}

	protected void OnMouseDrag()
	{
		camsettings.SetPanningEnable(false);
		if (isDraggable)
		{
			
			GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);
		
			foreach (GameObject go in dragTagObjs)
			{
				SelectableObject so = go.GetComponent<SelectableObject>();
				if (so != null)
				{
					
					Vector3 cursorPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, distFromCam);
					Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(cursorPoint) + so.offset;
					Rigidbody rb = go.GetComponent<Rigidbody>();

					if (so.isDraggable)
					{
						if (rb != null && !rb.isKinematic)
						{
							rb.velocity = (cursorPosition - transform.position) * 50;
							
						}
						else
						{
							go.transform.position = cursorPosition;
						}
					}
					
					
				}

			}
		}

	}

	protected void OnMouseUp()
	{
		if (!alphaMode)
		{
			// snap back to orig position if not > snapback distance
			GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);
			foreach (GameObject go in dragTagObjs)
			{
				SelectableObject so = go.GetComponent<SelectableObject>();
				Dropable dp = go.GetComponent<Dropable>();

				if (dp == null)
				{
					float dist = Vector3.Distance(go.transform.localPosition, so.origPos);
					if ((dist < snapbackDistMin || dist > origSnapbackDistMax) && so.isDraggable)
					{
						so.ResetPosition();
					}
				}
				
			}

			SetFreezeRigidBody(true);
		}
		camsettings.SetPanningEnable(true);
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

		Shader outline = Shader.Find("Outlined/Diffuse");

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

						m.shader = outline;

						// intensity of selectedColor determined by selectedColor alpha 

						Color emit = Color.Lerp(origMat.color, selectedColor, selectedColor.a);
						c_emit = emit * Mathf.LinearToGammaSpace(selectedColor.a);
						c = origMat.color;

						StandardShaderUtils.ChangeRenderMode(m, StandardShaderUtils.BlendMode.Opaque);

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

		//Debug.Log(state);
	}

	private void SetFreezeRigidBody(bool set)
	{
		Rigidbody rb = transform.GetComponent<Rigidbody>();
		if (rb)
		{
			if (set)
			{
				rb.constraints = RigidbodyConstraints.FreezeAll;
			}
			else
			{
				rb.constraints = RigidbodyConstraints.FreezeRotation;
			}
		}
		
	}

	public void AddRigidBody()
	{
		if (gameObject.GetComponent<Rigidbody>() == null)
		{
			Rigidbody rb = gameObject.AddComponent<Rigidbody>();
			rb.drag = 1;
			rb.angularDrag = 10000000000;
			SetFreezeRigidBody(true);
		}
		

	}

	public bool IsAtOrigPos() {
		bool atOrig = transform.localPosition == origPos;
		return atOrig;
	}
}