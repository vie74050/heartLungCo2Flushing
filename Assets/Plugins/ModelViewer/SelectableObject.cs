using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

/* multi-select ability by pressing SHIFT -- adds the tag Draggable to objects 

NB: MATERIALS should use STANDARD shaders.  Do not use Fade mode for transparent items, use Transparent mode instead.

Standard shader Fade mode reserved for setting visibility of non-selected parts

*/
[RequireComponent(typeof(Collider))]
public class SelectableObject : MonoBehaviour
{
	
	[DllImport("__Internal")]
	private static extern void BrowserSelect(string str);

	[Tooltip("if true, object is hilighted ")]
	public bool isSelected = false;

	[Tooltip("if true, object is transparent when deselected")]
	public bool alphaMode = false;			

	[Tooltip("Set the hilight tint")]
	public Color highlighColour = new Color(250f,250f,30f,.01f);

	// draggable vars
	public float snapbackDist = 5;
	public bool snaptoTarget = false;
    public Vector3 targetPos = new Vector3(0,10000,0);              // local position ref
	public bool isDraggable = true;									// set to make it draggable 

	//private Transform pivotObj; 									// adjust for wrong pivot points
	private Material[] origMaterials; 

	private Material mat_transparent;			

	private Vector3 screenPoint;

	private Vector3 offset;

	private Vector3 origPos;  		        						// local position relative to parent

	private Vector3 origScale;

    private animationOverride ao;

	private string tag_drag = "Draggable";

	private KGFOrbitCam camsettings;

	private void Start() {
		camsettings = Camera.main.GetComponent<KGFOrbitCam> ();

		// make sure transparent material is in the Resources/materials and shaders folder
		mat_transparent = Resources.Load("materials and shaders/transparent") as Material;

		// save orig materials
		if (GetComponent<Renderer>()) {
			int i = 0;
			origMaterials = GetComponent<Renderer>().materials;
			foreach(Material m in GetComponent<Renderer>().materials){	// store object's original materials
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
		

		// adjusted pivot ref if any
		Transform[] pivot = GetComponentsInChildren<Transform>();
		//pivotObj = pivot[pivot.Length-1];

		// draggable: save orig positions
		origPos = transform.localPosition;	
		origScale = transform.localScale;

		if (alphaMode) {
			ChangeMaterialColor ("transparent");
		}
		if (isDraggable) {
			if (GetComponent<Collider>() == null){
				gameObject.AddComponent<MeshCollider>();		
			}
		}

        if (gameObject.GetComponent<animationOverride>())
        {
            ao = gameObject.GetComponent<animationOverride>();
        }else{
            ao = new animationOverride();
            ao.animOverride = false;
        }
	}

	void Update(){
        float speed = 10f;
        if (snaptoTarget && !ao.animOverride) {
            
            transform.localPosition = Vector3.MoveTowards (transform.localPosition, targetPos, speed * Time.deltaTime);

		}
		if (isSelected) {
			gameObject.BroadcastMessage("e_selected");
		}

		if (!Input.GetMouseButton(0)){ 
			camsettings.SetPanningEnable (true);
		}
	}

	public void SetTargetPos(Transform target){

		//target.parent = transform.parent;
		targetPos = target.localPosition;
		//Destroy(target.gameObject);

	}
	public void TriggerAnim(){

		if (ao) {
            ao.PlayAnim();
		} 
	}
	public Material[] OriginalMaterials(){
		return origMaterials;	
	}

	public void Deselect(){
		gameObject.BroadcastMessage("e_deselected");
		if(alphaMode)
			ChangeMaterialColor("transparent");
		else
			ChangeMaterialColor("deselect");

		gameObject.tag = "Untagged";

		isSelected = false;

		//Debug.Log ("changed - deselect");
	}
	
	public void Select(){
		
		//Debug.Log ("changed - select" + transform.name);
		ChangeMaterialColor("select");
		gameObject.tag = tag_drag;
		isSelected = true;

		// broadcast to web container -- as of Unity 5.6 legacy way
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			Application.ExternalCall ("FromUnity_Select", transform.name);

			#if UNITY_WEBGL && !UNITY_EDITOR
			BrowserSelect(transform.name);
			#endif

		}

	}
	public void ResetSO(){
		transform.localPosition = origPos;
        snaptoTarget = false;
	}

	private void e_deselected(){
		
	}
	private void e_selected(){
		
	}

	private void OnMouseDown ()
	{
		Deselect();
		GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);
		//if ( isSelected && Input.GetKey(KeyCode.LeftShift) ){
		if ( isSelected ){
			


		}else{
			// if not SHIFT -- deselect all others and start new selection
			if (! Input.GetKey(KeyCode.LeftShift) && gameObject.tag != tag_drag) {				
				foreach (GameObject go in dragTagObjs){
					SelectableObject so = go.GetComponent<SelectableObject>();
					if (so !=null) so.Deselect();

				}			
			}

			Select();
				
		}

		// save positions for drag reference
		dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);	
		foreach (GameObject go in dragTagObjs){
			SelectableObject md = go.GetComponent<SelectableObject>();
			if (md != null ){
				md.screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);
				md.offset = go.transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, md.screenPoint.z));
			}
		}
	}
	
	private void OnMouseDrag(){

		// if snapback distance is set to 0, object can't be moved

		if (snapbackDist != 0 && isDraggable) {

			// prevent main camera drag event
			camsettings.SetPanningEnable(false);

			GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag (tag_drag);
		
			foreach (GameObject go in dragTagObjs) {
				SelectableObject so = go.GetComponent<SelectableObject> ();
				if (so != null) {
					Vector3 cursorPoint = new Vector3 (Input.mousePosition.x, Input.mousePosition.y, so.screenPoint.z);
					Vector3 cursorPosition = Camera.main.ScreenToWorldPoint (cursorPoint) + so.offset;
	
					go.transform.position = cursorPosition;	


				}
			
			}
		}
	}
	 
	private void OnMouseUp(){
		
		GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag(tag_drag);
		foreach (GameObject go in dragTagObjs) {
			SelectableObject so = go.GetComponent<SelectableObject>();
			float dist = Vector3.Distance(go.transform.localPosition, so.origPos);
			if (dist < snapbackDist) so.ResetSO ();
		}
			
	}

	private void ChangeMaterialColor(string state) {

		if (origMaterials == null)
			return;

		// using standard shader 

	  	GameObject obj = gameObject;

		Shader outline = Shader.Find ("Outlined/Diffuse");

		Color selectedColor = highlighColour; 

		Material[] materials = GetComponent<Renderer>().materials;

		int mat_i = 0;
		foreach (Material m in materials) {
			Material origMat = origMaterials [mat_i];
			Color c = origMat.color;
			Color c_emit = Color.black * Mathf.LinearToGammaSpace (0.9f);
				
			switch (state){

				case "select":
					
					m.shader = outline;
					
					// intensity of selectedColor determined by selectedColor alpha 
					
					Color emit =  Color.Lerp(origMat.color, selectedColor, selectedColor.a ); 
					c_emit = emit * Mathf.LinearToGammaSpace (selectedColor.a);	
					c = origMat.color;

                    StandardShaderUtils.ChangeRenderMode(m, StandardShaderUtils.BlendMode.Opaque);
					
					break;

				case "transparent":
					
						m.shader = mat_transparent.shader;
						c.a = (mat_transparent.color.a);
						StandardShaderUtils.ChangeRenderMode (m, StandardShaderUtils.BlendMode.Fade);   
							
						break;

				default:
						
						c = origMat.color;
						m.shader = origMat.shader;

						StandardShaderUtils.ChangeRenderMode(m, (StandardShaderUtils.BlendMode)origMat.GetFloat("_Mode") );

						break;
			}

			m.SetColor ("_Color", c);
			m.SetColor ("_EmissionColor", c_emit);
			m.color = c;
			mat_i++;
							
		}
			
	}

}

