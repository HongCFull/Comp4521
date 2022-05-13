using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "newCombatTerrain",menuName = "createNewCombatTerrain")]
public class CombatTerrain :ScriptableObject
{
    [SerializeField] private string terrainName;

    [SerializeField] private GameObject terrainPrefab;
    
    
}
