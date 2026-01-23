using UnityEngine;
/* Changes the color of a specified material globally */

public class GlobalMaterialColorChanger : MonoBehaviour
{
    [Tooltip("The material whose color will be changed globally.")]
    [SerializeField] private Material targetMaterial;     
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
    public void ChangeGlobalMaterialColor(Color color)
    {
        if (targetMaterial != null)
        {
            targetMaterial.color = color;
        }
        else
        {
            Debug.LogWarning("Target material is not assigned.");
        }
    }
}