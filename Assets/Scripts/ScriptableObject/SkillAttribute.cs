using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects/SkillAttribute", order = 1)]
public class SkillAttribute : ScriptableObject{
    public enum TargetType {
        Single,
        Multiple
    }
    public enum SkillType {
        Attack,
        Buff,
        Heal
    }
    public enum AnimationType {
        Melee,
        Ranged,
        Area,
        Buff
    }
    
    [SerializeField] private TargetType targetType;
    [SerializeField] private SkillType skillType;
    [SerializeField] private AnimationType animationType;
    public List<Vector2Int> affectedTiles { get; private set; }= new List<Vector2Int>();
    public List<Vector2Int> RotatedTargetTiles(int rotatedQuadrants) {
        // Quadrant 0: unrotated
        // Quadrant 1: Anti-Clockwise 90 degrees
        // Quadrant 2: Clockwise 180 degrees
        // Quadrant 3: Clockwise 90 degrees
        List<Vector2Int> tiles = new List<Vector2Int>();
        switch(rotatedQuadrants) {
            case 0:
                return new List<Vector2Int>(affectedTiles);
            case 1:
                foreach(Vector2Int tile in affectedTiles) {
                    tiles.Add(new Vector2Int(-tile.y, tile.x));
                }
                break;
            case 2:
                foreach(Vector2Int tile in affectedTiles) {
                    tiles.Add(new Vector2Int(-tile.x, -tile.y));
                }
                break;
            case 3:
                foreach(Vector2Int tile in affectedTiles) {
                    tiles.Add(new Vector2Int(tile.y, -tile.x));
                }
                break;
        }
        return tiles;
    }
    public List<Vector2Int> CalculateHitTiles(Vector2Int casterPosition, int rotatedQuadrants) {
        List<Vector2Int> tiles = new List<Vector2Int>();
        List<Vector2Int> rotatedTiles = RotatedTargetTiles(rotatedQuadrants);
        foreach(Vector2Int tile in rotatedTiles) {
            tiles.Add(tile + casterPosition);
        }
        return tiles;
    }
}