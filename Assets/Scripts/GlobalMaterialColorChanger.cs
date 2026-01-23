using UnityEngine;
/* Changes the color of a specified material globally */

public class GlobalMaterialColorChanger : MonoBehaviour
{
    [Tooltip("The material whose color will be changed globally.")]
    public Material targetMaterial;     
    [Tooltip("Target color to change the material to.")]
    public Color targetColor;

    public Color originalColor { get; private set; }
    private void Awake()
    {
        if (targetMaterial != null)
        {
            originalColor = targetMaterial.color;
        }
        else
        {
            Debug.LogWarning("Target material is not assigned.");
        }
    }
    public void ChangeGlobalMaterialColor()
    {
        if (targetMaterial != null)
        {
            targetMaterial.color = targetColor;
        }
        else
        {
            Debug.LogWarning("Target material is not assigned.");
        }
    }
    public void ResetMaterialColor()
    {
        if (targetMaterial != null)
        {
            targetMaterial.color = originalColor;
        }
        else
        {
            Debug.LogWarning("Target material is not assigned.");
        }
    }   
    void OnApplicationQuit()
    {
        ResetMaterialColor();
    }

}