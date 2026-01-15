/* Place on a pivotor game object to control the translation of target object along an axis (scale) */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Pivoter))]
public class MeterIndicator : MonoBehaviour
{
    [Tooltip("The target object to control (e.g. a marker or indicator)")]
    public Transform targetObj;

    [Tooltip("The position offset from the controller to place the target object if pivoted open")]
    public Vector3 positionOffset;

    private Vector3 initialLocalPos;

    private void Start() {
        initialLocalPos = targetObj.localPosition;

        if (TryGetComponent<Pivoter>(out Pivoter pivoter))
        {
            if (pivoter.isOpen)
            {
                SetIndicatorPosition(1f);
            }
            else
            {
                SetIndicatorPosition(0f);
            }
        }
    }

    private void SetIndicatorPosition(float t)
    {
        if (targetObj != null)
        {
            targetObj.localPosition = Vector3.Lerp(initialLocalPos, initialLocalPos + positionOffset, t);
        }
    }
    private void Update() {
        if (TryGetComponent<Pivoter>(out Pivoter pivoter))
        {
            float t = pivoter.isOpen ? 1f : 0f;
            SetIndicatorPosition(t);
        }
    }
}