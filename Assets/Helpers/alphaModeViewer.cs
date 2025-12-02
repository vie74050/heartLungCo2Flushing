using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

public class alphaModeViewer : MonoBehaviour {
    /// <summary>
    /// add script to game object to make it and children transparent 
    /// 
    /// NB: 
    /// 1) game object and children using Standard Shader
    /// 2) This changes Standard shader to use Fade mode for setting transparency. 
	///    This leaves transparent mode available for actual transparent material (e.g. glass).
	/// 3) Build for webGL -- include the transparent texture (Resources>materials and shaders)
	///    and set the material as a preloaded asset in HTML5 Player Settings
    /// </summary>

	// from jslib to communicate with browser javascript
	[DllImport("__Internal")]
	private static extern void BrowserApplicationStarted();

	[DllImport("__Internal")]
	private static extern void BrowserSelect(string str);

	public float alphaAmt = .1f;

	[Tooltip("parts that are shown -- not faded")]
	public GameObject[] selectedParts;			// must be child parts that will be extracted and have SelectableObject added

	public bool guiOption; 						// whether to show GUi controls for this

	private GameObject __selectedHolder__;
	private Material mat_transparent;			// transparent material must be in Resources folder; set as preloaded asset to make it gets packaged for webGL
	private List<Material> origMaterials;
	private KGFOrbitCam camsettings;
	private GUISkin _skin;

	private void Awake(){
		// create holder for selected parts 
		__selectedHolder__ = new GameObject("__selectedHolder__");
		__selectedHolder__.transform.parent = gameObject.transform.parent;

		//AssignSelectedParts (selectedParts);
		//SetSelectedParts("Frontal Bone");

	}

	private void Start()
    {
        
		if (Application.platform == RuntimePlatform.WebGLPlayer) {
			//Application.ExternalCall ("FromUnity_ApplicationStarted", true);
			#if UNITY_WEBGL && !UNITY_EDITOR
			BrowserApplicationStarted();
			#endif
		}

		Renderer[] objRenderers = GetComponentsInChildren<Renderer>();
		camsettings = Camera.main.GetComponent<KGFOrbitCam> ();

		if (_skin == null) {
			_skin = Resources.Load ("default") as GUISkin;
		} 

		mat_transparent = Resources.Load ("materials and shaders/transparent") as Material;

        origMaterials = new List<Material> ();

        foreach (Renderer r in objRenderers)
        {
            foreach (Material m in r.materials)
            {
				Material origm = new Material(m.shader);
				m.EnableKeyword("_ALPHABLEND_ON");
                m.EnableKeyword("_EMISSION");
                m.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                m.EnableKeyword("_Color");
                m.EnableKeyword("_Mode");

				origMaterials.Add(origm);

            }
        }
		setFade ();
	}

	private void OnGUI(){
		GUI.skin = _skin;
		if (guiOption) {
			GUILayout.BeginHorizontal ();

			if (GUILayout.Button ("Reset")) {
				SelectableObject[] sos = __selectedHolder__.gameObject.GetComponentsInChildren<SelectableObject> ();
				foreach (SelectableObject so in sos) {
					so.Reset ();
					so.Deselect ();
				}

			}

			GUILayout.Label ("Background transparency");

			GUILayout.EndHorizontal ();
			alphaAmt = GUILayout.HorizontalSlider (alphaAmt, 0, 1);
			if (GUI.changed) {
				setFade ();
				camsettings.SetPanningEnable (false);
			} else {
				camsettings.SetPanningEnable (true);
			}
		}
	}

	// provides a method for Web Container to set the selected parts 
	public void SetSelectedParts(string nameofObjects){
		bool nonefound = true;
		string[] arr = nameofObjects.Split(',');

		GameObject[] sp = new GameObject[ arr.Length ];
		for (int i = 0; i < arr.Length; i++) {
			string n = arr[i];
			GameObject go = GameObject.Find (n);

			if (go != null) {
				nonefound = false;
				sp [i] = go;
			}
		}

		if (nonefound) {
			// default to all
			sp = new GameObject[1];
			sp [0] = gameObject;

		}
		AssignSelectedParts (sp);
	}
	private void AssignSelectedParts( GameObject[] parts ){
		
		foreach (GameObject go in parts) {
			Renderer[] rds = go.GetComponentsInChildren<Renderer>();
			go.transform.parent = __selectedHolder__.transform;

			foreach (Renderer objRenderer in rds) {
				objRenderer.gameObject.AddComponent<SelectableObject>();
			}
		}
	}

	public void resetMaterials(){
        //revert to standard shader
        Renderer[] objRenderers = GetComponentsInChildren<Renderer>();

        int mat_i = 0;

        foreach (Renderer r in objRenderers)
        {
            foreach (Material m in r.materials)
            {
                Material origMat = origMaterials[mat_i];
                Color c = origMat.color;
               
                m.shader = origMat.shader;

                StandardShaderUtils.ChangeRenderMode(m, StandardShaderUtils.BlendMode.Opaque);
                mat_i++;
            }
        }
    }

    public void setFade(){
        Renderer[] objRenderers = GetComponentsInChildren<Renderer>();
      
		int mat_i = 0;

        foreach (Renderer r in objRenderers){
            foreach (Material m in r.materials)
            {
				Material origMat = origMaterials[mat_i];
				Color c = mat_transparent.color;
				m.shader = mat_transparent.shader;

                c.a = alphaAmt;
				m.SetColor("_Color", c);

				StandardShaderUtils.ChangeRenderMode(m, StandardShaderUtils.BlendMode.Fade);

				if (alphaAmt > .9f) {
					 
					// switch back to standard shader
					c = origMat.color;
					m.shader = origMat.shader;
					StandardShaderUtils.ChangeRenderMode(m, StandardShaderUtils.BlendMode.Opaque);
				}

				mat_i++;
					
            }
        }
       
       

    }

}
