using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Animation))]
public class animationOverride : MonoBehaviour {
    // make sure animation wrap mode set to Once

	[Tooltip("Does animation over-ride activity behaviour")]
	public bool animOverride = true;
	
    [HideInInspector] 
	public Animation anim;
		
	void Start() {
		anim = gameObject.GetComponent<Animation>();
        anim.playAutomatically = false;
	}

	public void PlayAnim(){
		anim.Play();
	}
    public void ResetAnim(){

       
        if (!anim.IsPlaying(anim.clip.name))
        {
            Debug.Log("reset anim");
            anim.Stop();
            anim.Rewind(anim.clip.name);
        }

    }
   

}
