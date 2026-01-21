/* Attache to game object `Main` to reset scene or load new scene */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    [Tooltip("Key to reset current scene in Play mode testing")]
    public KeyCode resetKey = KeyCode.R;

    void Update()
    {
        // for testing
        #if UNITY_EDITOR
        if (Input.GetKeyDown(resetKey))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        #endif
    }

    // for webGL Browser to Unity using UnityInstance.SendMessage('Main', 'ResetScene');
    // the game object must be named `Main`
    public void ResetScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}