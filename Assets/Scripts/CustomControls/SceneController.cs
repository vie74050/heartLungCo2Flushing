/* Attache to game object `Main` to reset scene or load new scene */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneController : MonoBehaviour
{
    [Tooltip("Key to reset current scene in Play mode testing")]
    public KeyCode resetKey = KeyCode.R;
    private float numFramesToWait = 60f;
    private int frameCount = 0;

    void Start()
    {
        frameCount = 0;
        // Create SceneFade object on scene start
        GameObject fadeObj = GetOrCreateFadeObject(out CanvasGroup cg);
        cg.alpha = 1f;
    }
    void Update()
    {
        // for testing
        #if UNITY_EDITOR
        if (Input.GetKeyDown(resetKey))
        {
            ResetScene();
        }
        #endif

        // fade in after 10 frames to allow scene to load
        if (frameCount == numFramesToWait)
        {
            StartCoroutine(FadeIn());
        }
        frameCount++;
    }

    // for webGL Browser to Unity using UnityInstance.SendMessage('Main', 'ResetScene');
    // the game object must be named `Main`
    public void ResetScene()
    {
        StartCoroutine(FadeAndReload());
        frameCount = 0;
    }

    private IEnumerator FadeAndReload()
    {
        yield return StartCoroutine(FadeOut());

        // Reload scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

    }

    private IEnumerator FadeOut()
    {
        GameObject fadeObj = GetOrCreateFadeObject(out CanvasGroup cg);

        // Fade in to black
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.5f; // 0.5 seconds fade
            cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeIn()
    {
        GameObject fadeObj = GameObject.Find("SceneFade");
        if (fadeObj == null)
            yield break;

        CanvasGroup cg = fadeObj.GetComponent<CanvasGroup>();
        float t = 1f;
        while (t > 0f)
        {
            t -= Time.unscaledDeltaTime / 0.5f;
            cg.alpha = Mathf.Clamp01(t);
            yield return null;
        }
        cg.alpha = 0f;
        Destroy(fadeObj);
    }

    private GameObject GetOrCreateFadeObject(out CanvasGroup cg)
    {
        GameObject fadeObj = GameObject.Find("SceneFade");
        if (fadeObj == null)
        {
            fadeObj = new GameObject("SceneFade");
            var canvas = fadeObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            fadeObj.AddComponent<CanvasRenderer>();
            cg = fadeObj.AddComponent<CanvasGroup>();
            var img = fadeObj.AddComponent<UnityEngine.UI.Image>();
            img.color = Color.black;
            img.rectTransform.anchorMin = Vector2.zero;
            img.rectTransform.anchorMax = Vector2.one;
            img.rectTransform.offsetMin = Vector2.zero;
            img.rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            cg = fadeObj.GetComponent<CanvasGroup>();
        }
        return fadeObj;
    }
}