using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

public enum GridMoveSet
{
    /// <summary>
    /// Obstacle has to be the LAST value!
    /// </summary>
    
    MeleeAttack,
    LongRangeAttack,
    SpecialSkill,
    UltimateSkill,
    Defense,
    Heal,
    PickUp,
    Obstacle
}

[System.Serializable]
[Tooltip("Row Col = 0,0 at the top left corner")]
public struct GridCoordinate : IEquatable<GridCoordinate>
{
    public int row;
    public int col;

    public GridCoordinate(int row,int col)
    {
        this.row = row;
        this.col = col;
    }
    public override bool Equals(object obj) => obj is GridCoordinate other && Equals(other);

    public bool Equals(GridCoordinate other) => other.row == this.row && other.col == this.col;
    
    public override string ToString() => string.Format("row = {0}, col = {1}",row,col);

    public override int GetHashCode()
    {
        HashCode hashCode = new HashCode();
        hashCode.Add(row);
        hashCode.Add(col);
        return hashCode.ToHashCode();
    }
}

public class BattleTerrain : MonoBehaviour
{
    [SerializeField] private Grid gridInfo;
    [SerializeField] private Transform topLeftTilePivot;
    
    [Tooltip("The maximum number of tiles in the (X-axis)")]
    [SerializeField] private int width;
    [Tooltip("The maximum number of tiles in the (Z-axis)")]
    [SerializeField] private int height;
    [SerializeField] private List<GridCoordinate> initObstacles;
    
    [Header("Distribution settings")] 
    [SerializeField] private int skillAWeight;
    [SerializeField] private int skillBWeight;
    [SerializeField] private int skillCWeight;
    [SerializeField] private int skillDWeight;
    [SerializeField] private int defenseWeight;
    [SerializeField] private int healWeight;

    public GridMoveSet[,] gridMoveSets { get; private set; }
    public Dictionary<GridCoordinate, TurnBasedActor> turnBasedActorCoord;

    private GridMoveSet[] stretchedGridMoveSets;
    private float cellWidth;
    private float cellHeight;
    
    void Awake()
    {
        gridMoveSets = new GridMoveSet[height,width];
        stretchedGridMoveSets = new GridMoveSet[height * width];
        turnBasedActorCoord = new Dictionary<GridCoordinate, TurnBasedActor>();
        
        cellWidth = gridInfo.cellSize.x;
        cellHeight = gridInfo.cellSize.z;
        
        RandomizeGridInfo();
        AssignInitialObstacle();
    }

    public GridMoveSet PopMoveSetAtGrid(GridCoordinate coord)
    {
        GridMoveSet moveSet = gridMoveSets[coord.row, coord.col];
        gridMoveSets[coord.row, coord.col] = GridMoveSet.Obstacle;
        return moveSet;
    }

    public void ReRollMoveSetAtGrid(GridCoordinate coord) => gridMoveSets[coord.row, coord.col] = GetRandomGridMoveSet();
    

    // Get the Grid world center position at the surface
    public Vector3 GetGridCenterPosByCoordinate(GridCoordinate coordinate)
    {
        NavMeshHit hit;
        Vector3 gridCenter=new Vector3(topLeftTilePivot.position.x + coordinate.col * cellWidth,
            topLeftTilePivot.position.y + cellWidth / 2, topLeftTilePivot.position.z - coordinate.row * cellHeight);
        NavMesh.SamplePosition(gridCenter, out hit, 5f,1);
        return hit.position;
    }

    public Vector3 GetWalkableGridCenterByWorldPos(Vector3 pos)
    {
        GridCoordinate closestGridCoord = GetGridCoordByWorldPos(pos);
        NavMeshHit hit;
        Vector3 gridCenter = new Vector3(topLeftTilePivot.position.x + closestGridCoord.col * cellWidth, pos.y, topLeftTilePivot.position.z-closestGridCoord.row * cellHeight);
        NavMesh.SamplePosition(gridCenter, out hit, 5f,1);
        return hit.position;
    }

    public GridCoordinate GetGridCoordByWorldPos(Vector3 pos)
    {
        int rowIndex = (int) Math.Round((topLeftTilePivot.position.z-pos.z) / cellHeight);
        int colIndex = (int) Math.Round((pos.x - topLeftTilePivot.position.x) / cellWidth);
        //Debug.Log("Get ：" +rowIndex+","+colIndex);
        return new GridCoordinate(rowIndex, colIndex);
    }

    public void RegisterTurnBasedActorOnGrid(TurnBasedActor actor, GridCoordinate coordinate)
    {
        if (turnBasedActorCoord.ContainsKey(coordinate)) {
            Debug.Log("coordinate: "+coordinate+" is occupied by actor: " + turnBasedActorCoord[coordinate].gameObject.name);
            return;
        }
        turnBasedActorCoord.Add(coordinate,actor);
        Debug.Log("Save actor : "+actor.gameObject.name+" in coord "+coordinate);
    }
    
    public void UnregisterActorOnGrid(TurnBasedActor actor, GridCoordinate coordinate)
    {
        if (!turnBasedActorCoord.ContainsKey(coordinate)) {
            Debug.Log("coordinate: "+coordinate+" is empty, cant unregister actor on it");
            return;
        }
        turnBasedActorCoord.Remove(coordinate);
        Debug.Log("Unregistered actor : "+actor.gameObject.name+" in coord "+coordinate);
    }
    
    void AssignInitialObstacle()
    {
        foreach (GridCoordinate coord in initObstacles) {
            gridMoveSets[coord.row, coord.col] = GridMoveSet.Obstacle;
        }
    }

    void RandomizeGridInfo()
    {
        InitStretchedGridInfo();
        RandomizeStretchedGridInfo();
        for (int h = 0; h < height; h++) {
            for (int w = 0; w < width; w++) {
                gridMoveSets[h, w] = stretchedGridMoveSets[w * h + h];
            }
        }
    }
    
    void InitStretchedGridInfo()
    {
        int totalWeightSum = skillAWeight + skillBWeight + skillCWeight + skillDWeight + defenseWeight + healWeight;
        int numOfSkillA = height * width* skillAWeight / totalWeightSum;
        int numOfSkillB = height * width* skillBWeight / totalWeightSum;
        int numOfSkillC = height * width* skillCWeight / totalWeightSum;
        int numOfSkillD = height * width* skillDWeight / totalWeightSum;
        int numOfDefense = height * width* defenseWeight / totalWeightSum;
        int numOfHeal = height * width* healWeight / totalWeightSum;

        for (int i = 0; i < height * width; i++) {
            if (numOfSkillA > 0) {
                stretchedGridMoveSets[i] = GridMoveSet.MeleeAttack;
                numOfSkillA--;
            }else if (numOfSkillB > 0) {
                stretchedGridMoveSets[i] = GridMoveSet.LongRangeAttack;
                numOfSkillB--;
            }else if (numOfSkillC > 0) {
                stretchedGridMoveSets[i] = GridMoveSet.SpecialSkill;
                numOfSkillC--;
            }else if (numOfSkillD > 0) {
                stretchedGridMoveSets[i] = GridMoveSet.UltimateSkill;
                numOfSkillD--;
            }else if (numOfDefense > 0) {
                stretchedGridMoveSets[i] = GridMoveSet.Defense;
                numOfDefense--;
            }else if (numOfHeal > 0) {
                stretchedGridMoveSets[i] = GridMoveSet.Heal;
                numOfHeal--;
            }else {
                stretchedGridMoveSets[i] = GridMoveSet.MeleeAttack;
            }
        }
    }
    
    void RandomizeStretchedGridInfo() {
        var rng = new System.Random();
        rng.Shuffle(stretchedGridMoveSets);
    }

    //Get a random GridMoveSet except Obstacle
    GridMoveSet GetRandomGridMoveSet()
    {
        GridMoveSet[] values = (GridMoveSet[]) Enum.GetValues(typeof(GridMoveSet));
        return values[new System.Random().Next(0,values.Length-1)];
    }

#if UNITY_EDITOR
    [ContextMenu("Show GridInfos")]
    public void ShowGridInfos()
    {
        gridMoveSets = new GridMoveSet[height,width];
        stretchedGridMoveSets = new GridMoveSet[height * width];
        RandomizeGridInfo();
        AssignInitialObstacle();
        
        showGridInfo = true;
      
    }
    
    [ContextMenu("Hide GridInfos")]
    public void HideGridInfos()
    {
        showGridInfo = false;
    }
    
    private bool showGridInfo = false;
    void OnDrawGizmos()
    {
        if (!showGridInfo)
            return;
        
        float tileWidth = gridInfo.cellSize.x;
        float tileHeight = gridInfo.cellSize.z;
        var centeredStyle = GUI.skin.GetStyle("Label");
        centeredStyle.alignment = TextAnchor.MiddleCenter;
        for (int h = 0; h < height; h++) {
            for (int w = 0; w < width; w++) {
                Vector3 tileWorldPos = topLeftTilePivot.position;
                tileWorldPos.x += tileWidth*w;
                tileWorldPos.z -= tileHeight * h;
                tileWorldPos.y += gridInfo.cellSize.y/2;
                Handles.Label(tileWorldPos,"["+h+","+w+"]",centeredStyle);

                if (gridMoveSets!=null) {
                    tileWorldPos.z += 0.5f;
                    Handles.Label(tileWorldPos,gridMoveSets[h,w].ToString());
                }

            }
        }
    }
#endif
}


