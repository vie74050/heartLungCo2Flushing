using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDropHandler
{
    void Init();
    void eDrop(Transform target);
}