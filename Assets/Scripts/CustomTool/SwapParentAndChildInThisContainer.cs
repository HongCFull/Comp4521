using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwapParentAndChildInThisContainer : MonoBehaviour
{
    [SerializeField] private List<Transform> parents;

    [ContextMenu("SwapParentAndChildRelationship")]
    public void SwapParentAndChildRelationship()
    {
        foreach (Transform parent in parents) {
            Transform child = parent.GetChild(0);
            child.parent = this.transform;
            parent.transform.parent = child;
        }
    }
    
}
