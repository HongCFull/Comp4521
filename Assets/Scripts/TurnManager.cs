using System.Collections.Generic;
using UnityEngine;

public class TurnManager : MonoBehaviour {

    [Header("Tile Settings")]
    [SerializeField] Vector2 tileSize;
    [SerializeField] Vector2 centerTile;

    [Header("Monsters Management")]
    public GameObject[] monstersList;

    // Turn Management
    List<Monster> existingMonsters = new List<Monster>();
    List<Monster> turnOrder = new List<Monster>();

    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                
            }
        }
    }

    Vector2Int WorldPosToTile(Vector3 pos) {
        return new Vector2Int(Mathf.RoundToInt((pos.x-centerTile.x)/tileSize.x), Mathf.RoundToInt((pos.z-centerTile.y)/tileSize.y));
    }

    // Monster Management
    void SpawnMonster(int monsterIndex) {
        if(monsterIndex < 0 || monsterIndex >= monstersList.Length) return;

        
    }
}
