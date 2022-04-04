using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CleanAllChildrenInObj : MonoBehaviour
{
    [SerializeField] private List<Transform > _gameObjectParent;

    [ContextMenu("DestroyAllGameObjectChildren")]
    void DestroyAllGameObjectChildren()
    {
        foreach (Transform parent in _gameObjectParent) {
            foreach (Transform child in parent) {
                DestroyImmediate(child.gameObject);
            }
        }
    }
}
