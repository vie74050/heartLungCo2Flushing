using UnityEngine;
/* script for controlling the object using the Fill_Y shader, 
i.e. to simulate fluid fill level 
Require: a Renderer with the Fill_Y shader applied.
Set the min and max Y values in the shader.
*/
public class Fill_Y : MonoBehaviour
{
    [Tooltip("Renderer with Fill_Y shader to control")]
    public Renderer fluidRenderer;
    [Range(0f, 1f)]
    public float fillPercent; // 0–1

    private float minY = 0f;
    private float maxY = 1f;

    Bounds bounds;

    void Start()
    {
        if (fluidRenderer == null)
        {
            fluidRenderer = GetComponent<Renderer>();
        }

        if (fluidRenderer == null)
        {
            Debug.LogError("Fill_Y script requires a Renderer with Fill_Y shader.");
            return;
        }
        bounds = fluidRenderer.bounds;
        minY = fluidRenderer.material.GetFloat("_ClipStartY");
        maxY = fluidRenderer.material.GetFloat("_ClipEndY");
    }
    void Update()
    {
        SetFillY(fillPercent);
    }
    public void SetFillY(float percent)
    {
        fillPercent = Mathf.Clamp01(percent);
        float fillAmt = fillPercent * (maxY - minY) + minY;
        fluidRenderer.material.SetFloat("_ClipEndY", fillAmt);
    }
}