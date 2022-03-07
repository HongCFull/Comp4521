
using UnityEngine;

public class TurnManager : MonoBehaviour {
    [Header("Tile Settings")]
    [SerializeField] Vector2 tileSize;
    [SerializeField] Vector2 centerTile;


    void Update() {
        if(Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if(Physics.Raycast(ray, out hit)) {
                Debug.Log(hit.point);
                Debug.Log(WorldPosToTile(hit.point));
            }
        }
    }
    Vector2Int WorldPosToTile(Vector3 pos) {
        return new Vector2Int(Mathf.RoundToInt((pos.x-centerTile.x)/tileSize.x), Mathf.RoundToInt((pos.z-centerTile.y)/tileSize.y));
    }
}
