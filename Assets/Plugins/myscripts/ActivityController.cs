using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

[RequireComponent(typeof(KGFOrbitCam))]

public class ActivityController : MonoBehaviour {
	

	// from jslib to communicate with browser javascript
	[DllImport("__Internal")]
	private static extern void BrowserApplicationStarted();

	/* set up options */

	[Tooltip("Required: the game object parent- anything not child of this will not be selectable")]
	public GameObject _MODEL;

	[Tooltip("Optional: distance from original position; part will snap back if under specified amount.  Use 0 = can't be removed ")]
	public float snapbackDist = 5;

	[Tooltip("Optional: if part name label is displayed when part selected")]
	public bool showlabels = false;

	[Tooltip("show or hide parts list")]
	public bool showPartsList = true;

	[Tooltip("Optional: tint color for selected items")]
	public Color selectedColor = Color.yellow;

    [Tooltip("Optional: show alpha mode toggle?")]
    public bool showAlphaMode = true;

	[Tooltip("Optional: start in alpha mode?")]
	public bool alphaMode = true;

	public Texture logo;

	private GUISkin _skin;

	// Task Activity gui vars

	[HideInInspector]
	public List<GameObject> activityList;

	[HideInInspector]
	public List<EditorGameObject> taskItems;

    [HideInInspector]
    public GameObject TargetEndPos;

	[HideInInspector]
	public bool showActivity = false;


	[HideInInspector]
	public bool showActivitySteps = false;

	[HideInInspector]
	public bool showLogBtn = false;

	[HideInInspector]
	public string activityTaskName = "Task";

	private static List<string> _activityLog;	
	private int activityCounter = 0;  	// keep track of what step has been done\

	private KGFOrbitCam camsettings;
	private Transform currentSelection ;

	// hierarchy list
	private List<ListItem> items;
	private int listId = 0; 

	private Dictionary<string, ListChecklistItem> taskList;

	/* gui vars */
	private bool overGUI = false;  // detect if mouse is over gui elements
	private Rect m_SelectionWindowRect = new Rect (600.0f, 10.0f, 120.0f, 50.0f);

	private bool showLog = false;


	// gui vars for parts list 
	private Vector2 scrollPosition = Vector2.zero;
	private Rect scrollviewOutter = new Rect(0, 0, 350, Screen.height);

	private int list_level = 0;

	// gui vars for activity list 
	private Vector2 scrollPosition2 = Vector2.zero;
	private Rect scrollviewOutter2 = new Rect(Screen.width-320, 0, 320, 200);

	// gui vars for log list 
	private Vector2 scrollPosition3 = Vector2.zero;
	private Rect scrollviewOutter3 = new Rect(Screen.width-320, Screen.height-200, 320, 200);

	private bool overpart = false;

	private Animation anim;

	void Awake(){
		//Debug.Log ("taskItems:" + taskItems.Count);
		foreach (EditorGameObject taskstep in taskItems) {
			if (taskstep.goRef != null) {
				activityList.Add (taskstep.goRef);
			}
		}
			
		if (_skin == null) {
			_skin = Resources.Load ("default") as GUISkin;
		} 

		// set up global behavious: camera, selection, colliders...etc.
		camsettings = Camera.main.GetComponent<KGFOrbitCam>();
		if (_MODEL == null)
			_MODEL = gameObject;

		_MODEL.SetActive (true);

        if (TargetEndPos){
            // hide target position
            TargetEndPos.SetActive (false);
        }

		Renderer[] rds = _MODEL.GetComponentsInChildren<Renderer>();

		foreach( Renderer objRenderer in rds){
			string partname = objRenderer.name; Debug.Log (partname);

			// add selectable object using default options to children of _MODEL if doesn't already have one
			SelectableObject so = objRenderer.gameObject.GetComponent<SelectableObject>();
			if (so == null) {
				objRenderer.gameObject.AddComponent<SelectableObject>();
				so = objRenderer.gameObject.GetComponent<SelectableObject>();
				so.snapbackDist = snapbackDist; //activity mode, default always snap back unless correct
				so.alphaMode = alphaMode;
				so.highlighColour = selectedColor;

                if (TargetEndPos){

					Transform[] targets = TargetEndPos.GetComponentsInChildren<Transform> ();
					foreach (Transform targetT in targets) {

						if (targetT.name == partname) {
							so.SetTargetPos (targetT);
							break;
						}
					}
                   
                }
			}

		};	
			
		items = new List<ListItem>();
		MakeListItems (_MODEL, 0);	
	}

	void Start(){
		
		_activityLog = new List<string>();
		_activityLog.Add ("Task Start: " + System.DateTime.Now );
		taskList = new Dictionary<string,ListChecklistItem>();
		if (showActivity) {
			MaketaskList ();
		}

		anim = _MODEL.GetComponent<Animation> ();

		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			//Application.ExternalCall ("FromUnity_ApplicationStarted", true);
			#if UNITY_WEBGL && !UNITY_EDITOR
			BrowserApplicationStarted();
			#endif
		}
			
	}

	void Update(){
		
		// prevent panning of camera if dragging an object
		if (Input.GetMouseButtonDown (0)) {
			
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);

			if (Physics.Raycast (ray, out hit)) {
				//print ("selecting or dragging part");
				overpart = true;
				Transform t = hit.transform;
				currentSelection = t;
			} else {
				overpart = false;
			}

		} 
			
		if (!Input.GetMouseButton(0)){ 
			camsettings.SetPanningEnable (true);
			currentSelection = null;
		}
		if ( !Input.GetMouseButton(0) && !Input.GetMouseButton(1) ){
			overpart = false;
		}
			

						
	}


	void OnGUI(){
		GUI.skin = _skin;

		GUILayout.BeginHorizontal();

		if (logo != null) {
			GUI.Label(new Rect(Screen.width - 100, 0, 100, 100), logo);
		}

		/**** global control buttons ***/

		// reset btn
		if ( GUILayout.Button("Reset") ){
			ResetAll ();
		}

		if (GUILayout.Button("Deselect All")) {
			GameObject[] dragTagObjs = GameObject.FindGameObjectsWithTag ("Draggable");
			foreach (GameObject go in dragTagObjs) {
				SelectableObject so = go.GetComponent<SelectableObject> ();
				so.Deselect ();

			}
		}

        // transparencey btn
        if (showAlphaMode)
        {
            alphaMode = GUILayout.Toggle(alphaMode, "Isolate", new GUIStyle("button"));
        }

		if (showPartsList) {
			items [0].selected = GUILayout.Toggle (items [0].selected, items [0].node.name, new GUIStyle ("button"));
		} else {
			items [0].selected = false;
		}

		// anim btn
		if (anim != null) {
			if ( GUILayout.Button("Expand") ){
				anim.Play ();
			}
		}

		GUILayout.EndHorizontal();

		// options
		if (showPartsList) {
			GUI_PartsList ();
		}


		if (showActivity) {
			// if not empty, display list
			GUI_ActivityList();
		}


		if (showlabels) {
			GUIStyle style;
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			string overPartname = "";
				
			if (Physics.Raycast (ray, out hit)) {
				Transform t = hit.transform;
				overPartname = t.name.Replace("_", " "); 
				// GUIStyle for part label
				Texture2D texture = new Texture2D (1, 1);
				style = new GUIStyle ();
				style.normal.background = texture;
				style.normal.textColor = Color.black;
				style.wordWrap = true;
				style.alignment = TextAnchor.MiddleCenter;
				style.padding = new RectOffset (3, 3, 5, 5);
				texture.SetPixel (1, 1, Color.white);
				texture.Apply ();
				if (overPartname != "") {
					m_SelectionWindowRect.x = Input.mousePosition.x + 30;
					m_SelectionWindowRect.y = Screen.height - Input.mousePosition.y - 30;
					m_SelectionWindowRect.height = style.CalcHeight (new GUIContent (overPartname), m_SelectionWindowRect.width);
					GUI.Label (m_SelectionWindowRect, overPartname, style);
				}
			}
		}

		if (showLog && showLogBtn) {
			GUI_TaskLog();
		}

		overGUI = overpart || (items[0].selected && scrollviewOutter.Contains(Event.current.mousePosition) )|| (showActivity && scrollviewOutter2.Contains(Event.current.mousePosition) ) || (showLog && scrollviewOutter3.Contains(Event.current.mousePosition) );

		camsettings.SetZoomEnable( !overGUI );
		camsettings.SetPanningEnable ( !overGUI );
		//camsettings.SetRotationEnable (!overGUI);


		if (GUI.changed) {
			foreach(ListItem li in items){
				if (li.so != null){
					li.so.alphaMode = alphaMode;
					if (!li.so.isSelected) {
						li.so.Deselect ();
					} else {
						li.so.Select ();
					}
				}

			}
           
		}

	}
			
	private void GUI_PartsList(){

		// parts list
		if (items[0].selected){
			
			scrollPosition = GUILayout.BeginScrollView(scrollPosition,  new GUIStyle("scrollview"), GUILayout.MaxWidth(320) );

			foreach(ListItem li in items){
				
				string pathname = li.objPathName;
				// object names should not have / in it
				list_level = pathname.Split('/').Length;

				if (li.showBtn) {

					if (li.so == null) { 
						
						// make group container
						GUILayout.BeginHorizontal ();
						GUILayout.Space (list_level * 10);

						if (li.id != 0) {
							Transform[] t = li.node.GetComponentsInChildren<Transform>();

							GUILayout.BeginHorizontal ();
							li.selected = GUILayout.Toggle (li.selected, li.node.name); 

							// select all children
							if ( GUILayout.Button("all", new GUIStyle ("smallbtn") ) ) {
								li.selected = true;

								foreach (Transform child in t) {
									SelectableObject so = child.GetComponent<SelectableObject> ();
									if (so!=null){
										so.Select ();
									}
								};
							}
							if ( GUILayout.Button("none", new GUIStyle ("smallbtn") ) ) {
								li.selected = false;
								foreach (Transform child in t) {
									SelectableObject so = child.GetComponent<SelectableObject> ();
									if (so!=null){
										so.Deselect ();
									}
								};
							}

							GUILayout.EndHorizontal ();
						}

						GUILayout.EndHorizontal ();
					} else {
						
						string btnstyle = (li.childIndices.Count > 0) ? "toggle_button_1" : "button_1";
						if (li.so.isSelected)
							btnstyle = "button_selected";
						
						li.so.alphaMode = alphaMode;

						// make button
						GUILayout.BeginHorizontal ();
						GUILayout.Space (list_level * 15);

						if (GUILayout.Button (li.node.name, new GUIStyle (btnstyle))) {
							
							li.selected = !li.so.isSelected;

							if (!li.selected) {
								li.so.Deselect ();
							} else {
								li.so.Select ();
							}		
						}	
						GUILayout.EndHorizontal ();
					}

					if (li.childIndices.Count > 0) {
						bool showchildren = li.selected && li.showBtn;

						if (li.so != null)
							showchildren = li.so.isSelected;


						foreach (int i in li.childIndices) {
							if (items [i].so != null) {
								items [i].showBtn = (items [i].so.isSelected) ? items [i].so.isSelected : showchildren;
							} else {
								items [i].showBtn = showchildren;
							}

						}

					}

				} else {
					
					foreach (int i in li.childIndices) {

						if (items [i].so != null) {
							li.showBtn = items [i].showBtn = items [i].so.isSelected;
							if (items [i].so.isSelected){

								break;
							}
						}

					}
				}

			}
			GUILayout.EndScrollView();
		}

		Rect scrollviewRect = GUILayoutUtility.GetLastRect();
		if (  scrollviewRect.width == scrollviewOutter.width ){
			scrollviewOutter.height = scrollviewRect.height;
		};
	}

	// handle calling events once vs every frame within ongui

	private void GUI_ActivityList(){
		string msg = "";
		string msg_style = "label";
		string listlabel = "";
		Rect activityArea = new Rect(0, 0, scrollviewOutter2.width, scrollviewOutter2.height + 150);
		activityArea.x = Screen.width - scrollviewOutter2.width;
		GUILayout.BeginArea (activityArea, new GUIStyle ("window"));

		GUILayout.BeginHorizontal ();
		/*if (!showActivitySteps) {
			if (GUILayout.Button ("Show Task Steps", GUILayout.ExpandWidth (true))) {
				showActivitySteps = !showActivitySteps;
			}
		}*/
		if (showLogBtn) {
			showLog = GUILayout.Toggle (showLog, "Log", new GUIStyle ("button"));
		}
		GUILayout.EndHorizontal ();
		GUILayout.Label (activityTaskName);

		scrollPosition2 = GUILayout.BeginScrollView(scrollPosition2, new GUIStyle ("box"), GUILayout.MaxWidth(scrollviewOutter2.width) );

		if (currentSelection != null) {
			GameObject currentGO = currentSelection.gameObject;

			listlabel = currentSelection.name;

			// target of current actitivy step 
			string activityListRef = (activityCounter < activityList.Count) ? activityList [activityCounter].name : "completed";

			// check if current select is the correct next step in task list
			if ( listlabel != activityListRef)  {
				SelectableObject so = currentGO.GetComponent<SelectableObject> ();
				// snap back all wrong items
				if (so != null && !so.snaptoTarget) {
					so.Reset (); 
					msg = "Incorrect part for this step. Try again!";
					msg_style = "wrong";

					if (_activityLog[_activityLog.Count-1] != listlabel + " - wrong") {
						_activityLog.Add (listlabel + " - wrong");
                    }

				}
			}



		}

		//do GUI for activity list
		Dictionary<string, ListChecklistItem>.KeyCollection keys = taskList.Keys;
		foreach (string s in keys) {
			ListChecklistItem item = taskList [s];
            SelectableObject so = item.go.GetComponent<SelectableObject>();

			if ( !item.complete && item.name == activityList [activityCounter].name){

				if (so != null ) {

					if (so.isSelected) {
                        
						so.snapbackDist = .01f;
						item.complete = true; 
						activityCounter++;
					}
				}
				if (item.complete) {
					_activityLog.Add("Completed task step " + item.go.name);
                    so.TriggerAnim();
				}
		
			}

            so.snaptoTarget = item.complete;

			listlabel = (showActivitySteps || item.complete) ? item.name : "";
			string liststyle = (showActivitySteps || item.complete) ? "toggle_checklist" : "label";

			if (listlabel != "") {
				GUILayout.Toggle (item.complete, listlabel, new GUIStyle (liststyle));
			}

		}

		if (activityCounter == activityList.Count) {
			msg = "Task Completed!";
			msg_style = "complete";

			if (_activityLog[_activityLog.Count-1] != msg) {
				
				_activityLog.Add (msg);
			}

		}
			
		GUILayout.EndScrollView ();	

		if (msg !="")
			GUILayout.Label (msg, new GUIStyle (msg_style));

		GUILayout.EndArea ();

	}


	private void GUI_TaskLog(){
		string guitext = "";

		Rect tasklogArea = scrollviewOutter3;
		tasklogArea.x = Screen.width - scrollviewOutter3.width;
		tasklogArea.y = Screen.height - scrollviewOutter3.height;

		GUILayout.BeginArea (tasklogArea, new GUIStyle ("box"));
		GUILayout.Label ("Event Log");

		scrollPosition3 = GUILayout.BeginScrollView(scrollPosition3, new GUIStyle ("box"), GUILayout.MaxWidth(scrollviewOutter3.width) );

		foreach (string s in _activityLog) {
			guitext += s + "\n";

			GUILayout.Label (s);
		}

		GUILayout.EndScrollView ();
		GUILayout.EndArea ();
	}

	private void ResetAll(){
		_activityLog.Add ("Task Restart: " + System.DateTime.Now );
		foreach(ListItem li in items){
			// reset list
			li.selected = false;
			li.showBtn = (li.parent_i == 0);

			// reposition model
			if (li.so){
				li.node.layer = 0;
				li.so.snapbackDist = snapbackDist;
				li.so.alphaMode = alphaMode;
				li.so.Deselect();
				li.so.Reset();
			}
		}


		Dictionary<string, ListChecklistItem>.KeyCollection keys = taskList.Keys;
		foreach (string s in keys) {
			ListChecklistItem item = taskList [s];
			item.complete = false;
		}
		activityCounter = 0;

		// reset camera
		camsettings.ApplyStartValues();
	}

	private void MakeListItems(GameObject go, int parent_id){

		ListItem node = new ListItem(listId, parent_id, go);
		items.Add(node);

		foreach(Transform t in go.transform) {

			listId++;
			node.AddChildRef(listId);
			MakeListItems(t.gameObject, node.id);	

		}
	}

	private void MaketaskList() {
		int n = activityList.Count;

		foreach (GameObject go in activityList) {
			
			ListChecklistItem node = new ListChecklistItem (go, null);

			taskList.Add(go.name,node);
		//	Debug.Log ("added " + go.name);

		}
	}
}
