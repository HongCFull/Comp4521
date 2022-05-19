using System.Collections.Generic;
using UnityEngine;
using Grid = System.Windows.Forms.DataVisualization.Charting.Grid;

[CreateAssetMenu(fileName = "New Skill", menuName = "ScriptableObjects/SkillAttribute", order = 1)]
public class SkillAttribute : ScriptableObject{
    public enum TargetType {
        Single,
        Directional,
        Centered
    }
    public enum SkillType {
        Attack,
        Buff,
        Heal
    }
    public enum AnimationType {
        Melee,
        Ranged,
        Areal,
        Buff
    }
    
    public TargetType targetType;
    public SkillType skillType;
    public AnimationType animationType;
    public bool requireUserInput;
    
    public List<Vector2Int> affectedTiles = new List<Vector2Int>();
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

    public List<GridCoordinate> GetPotentialCastingPoint(GridCoordinate center,int numOfRowInMap,int numOfColInMap)
    {
        List<GridCoordinate> validCastGrids = new List<GridCoordinate>();
        for(int i = 0; i < 4; i++) {
            foreach(Vector2Int target in RotatedTargetTiles(i)) {
                int row = center.row + target.x;
                int col = center.col + target.y;
                if(row>0 && row<numOfRowInMap && col>0 && col<numOfColInMap)
                    validCastGrids.Add(new GridCoordinate(row,col));
            }
        }
        return validCastGrids;
    }
}
