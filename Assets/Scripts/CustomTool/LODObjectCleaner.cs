using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomTool
{
    public class LODObjectCleaner : MonoBehaviour
    {
        [SerializeField] private List<LODGroup > lodGroups;

        /// <summary>
        /// Only keep the LOD 0 object (only works if the object name has "LOD0")
        /// </summary>
        [ContextMenu("Only Keep LOD 0 object")]
        void KeepLOD0ObjectOnly()
        {
            List<GameObject> objectsToBeDestoryed = new List<GameObject>();
            foreach (LODGroup lodObject in lodGroups) {
                foreach (Transform child in lodObject.gameObject.transform) {
                    //Debug.Log(child.gameObject.name);
                    if (!child.gameObject.name.Contains("LOD0")) //destroy all non LOD0 object{{
                        objectsToBeDestoryed.Add(child.gameObject);
                }
            }

            foreach (GameObject obj in objectsToBeDestoryed) {
                DestroyImmediate(obj,true);
            }

        }

        [ContextMenu("RemoveLODComponentOfAssets")]
        void RemoveLODComponentOfAssets()
        {
            foreach (LODGroup lodObject in lodGroups) {
                DestroyImmediate(lodObject,true);
            }
            lodGroups.Clear();
        }
        
    }
}
