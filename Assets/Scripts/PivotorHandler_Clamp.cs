using UnityEngine;
// requires Pivoter on same object
[RequireComponent(typeof(Pivoter))]

public class Clamp : MonoBehaviour
{
    [Tooltip("The hoses to unclamp/clamp in response to pitorer open/close")]
    public HoseFlow[] hosesToClamp;
    
    private Pivoter pivoter;
    private void Start()
    {
        pivoter = GetComponent<Pivoter>();
        DoClamping();
    }

    private void OnMouseDown()
    {
        DoClamping();
    }

    private void DoClamping()
    {
        foreach (HoseFlow hose in hosesToClamp)
        {
            if (pivoter.isOpen)
            {
                hose.SetClamp(false);
            }
            else
            {
                hose.SetClamp(true);
            }
        }
    }
}