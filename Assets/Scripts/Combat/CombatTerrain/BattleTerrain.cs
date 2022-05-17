using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common.CommandTrees.ExpressionBuilder;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

using static MoveSetOnGrid;

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
    [Header("Grid Setting")]
    [SerializeField] private Grid gridInfo;
    [SerializeField] private Transform topLeftTilePivot;
    [SerializeField] private List<GridCoordinate> initObstacles;
    
    [Tooltip("The maximum number of tiles in the (X-axis)")]
    [SerializeField] private int numCol;
    [Tooltip("The maximum number of tiles in the (Z-axis)")]
    [SerializeField] private int numRow;
    
    [Header("Distribution settings")] 
    [SerializeField] private int skillAWeight;
    [SerializeField] private int skillBWeight;
    [SerializeField] private int skillCWeight;
    [SerializeField] private int skillDWeight;
    [SerializeField] private int defenseWeight;
    [SerializeField] private int healWeight;

    [Header("References")]
    [SerializeField] BattleTerrainCanvas battleTerrainCanvas;
    public MoveSetType[,] gridMoveSets { get; private set; }
    public Dictionary<GridCoordinate, TurnBasedActor> turnBasedActorCoord;

    private MoveSetType[] stretchedGridMoveSets;
    private float cellWidth;    //x
    private float cellHeight;   //y
    private float cellLength;   //z
    
    void Awake()
    {
        gridMoveSets = new MoveSetType[numRow,numCol];
        stretchedGridMoveSets = new MoveSetType[numRow * numCol];
        turnBasedActorCoord = new Dictionary<GridCoordinate, TurnBasedActor>();
        
        cellWidth = gridInfo.cellSize.x;
        cellLength = gridInfo.cellSize.z;
        
    }

///======================================================================================================================================================
///Public Method:
///======================================================================================================================================================
    public void InitializeBattleTerrain()
    {
        RandomizeGridInfo();
        AssignInitialObstacle();
        InitializeMoveSetUIOnGrids();
    }

    public MoveSetType PopMoveSetAtGrid(GridCoordinate coord)
    {
        MoveSetType moveSet = gridMoveSets[coord.row, coord.col];
        gridMoveSets[coord.row, coord.col] = MoveSetType.Obstacle;
        battleTerrainCanvas.UpdateMoveSetImageAtGridTo(coord,MoveSetType.Obstacle);
        return moveSet;
    }

    public void SetMoveSetAtGridTo(GridCoordinate coord, MoveSetType moveSetType)
    {
        gridMoveSets[coord.row, coord.col] = moveSetType;
        battleTerrainCanvas.UpdateMoveSetImageAtGridTo(coord,moveSetType);
    }

    public void ReRollMoveSetAtGrid(GridCoordinate coord) {
        MoveSetType moveSetType = GetRandomGridMoveSet();
        gridMoveSets[coord.row, coord.col] = moveSetType;
        battleTerrainCanvas.UpdateMoveSetImageAtGridTo(coord,moveSetType);
    }

    // Get the Grid world center position at the surface
    public Vector3 GetGridCenterOnNavmeshByCoord(GridCoordinate coordinate)
    {
        NavMeshHit hit;
        Vector3 gridCenter=new Vector3(topLeftTilePivot.position.x + coordinate.col * cellWidth,
            topLeftTilePivot.position.y + cellWidth / 2, topLeftTilePivot.position.z - coordinate.row * cellLength);
        NavMesh.SamplePosition(gridCenter, out hit, 5f,1);
        return hit.position;
    }

    public Vector3 GetGridCenterOnNavmeshByWorldPos(Vector3 pos)
    {
        GridCoordinate closestGridCoord = GetGridCoordByWorldPos(pos);
        NavMeshHit hit;
        Vector3 gridCenter = new Vector3(topLeftTilePivot.position.x + closestGridCoord.col * cellWidth, pos.y, topLeftTilePivot.position.z-closestGridCoord.row * cellLength);
        NavMesh.SamplePosition(gridCenter, out hit, 5f,1);
        return hit.position;
    }

    public GridCoordinate GetGridCoordByWorldPos(Vector3 pos)
    {
        int rowIndex = (int) Math.Round((topLeftTilePivot.position.z-pos.z) / cellLength);
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


///======================================================================================================================================================
///Private Method:
///======================================================================================================================================================

    void RandomizeGridInfo()
    {
        InitStretchedGridInfo();
        RandomizeStretchedGridInfo();
        for (int h = 0; h < numRow; h++) {
            for (int w = 0; w < numCol; w++) {
                gridMoveSets[h, w] = stretchedGridMoveSets[w * h + h];
            }
        }
    }
    
    void InitStretchedGridInfo()
    {
        int totalWeightSum = skillAWeight + skillBWeight + skillCWeight + skillDWeight + defenseWeight + healWeight;
        int numOfSkillA = numRow * numCol* skillAWeight / totalWeightSum;
        int numOfSkillB = numRow * numCol* skillBWeight / totalWeightSum;
        int numOfSkillC = numRow * numCol* skillCWeight / totalWeightSum;
        int numOfSkillD = numRow * numCol* skillDWeight / totalWeightSum;
        int numOfDefense = numRow * numCol* defenseWeight / totalWeightSum;
        int numOfHeal = numRow * numCol* healWeight / totalWeightSum;

        for (int i = 0; i < numRow * numCol; i++) {
            if (numOfSkillA > 0) {
                stretchedGridMoveSets[i] = MoveSetType.Melee;
                numOfSkillA--;
            }else if (numOfSkillB > 0) {
                stretchedGridMoveSets[i] = MoveSetType.Directional;
                numOfSkillB--;
            }else if (numOfSkillC > 0) {
                stretchedGridMoveSets[i] = MoveSetType.AOE;
                numOfSkillC--;
            }else if (numOfSkillD > 0) {
                stretchedGridMoveSets[i] = MoveSetType.UltimateSkill;
                numOfSkillD--;
            }else if (numOfDefense > 0) {
                stretchedGridMoveSets[i] = MoveSetType.Defense;
                numOfDefense--;
            }else if (numOfHeal > 0) {
                stretchedGridMoveSets[i] = MoveSetType.Heal;
                numOfHeal--;
            }else {
                stretchedGridMoveSets[i] = MoveSetType.Melee;
            }
        }
    }
    
    void RandomizeStretchedGridInfo() {
        var rng = new System.Random();
        rng.Shuffle(stretchedGridMoveSets);
    }

    void AssignInitialObstacle()
    {
        foreach (GridCoordinate coord in initObstacles) {
            gridMoveSets[coord.row, coord.col] = MoveSetOnGrid.MoveSetType.Obstacle;
        }

    }

    void InitializeMoveSetUIOnGrids() => battleTerrainCanvas.InitializeTileImages(gridMoveSets,numRow,numCol);

    
    //Get a random GridMoveSet except Obstacle
    MoveSetType GetRandomGridMoveSet()
    {
        MoveSetType[] values = (MoveSetType[]) Enum.GetValues(typeof(MoveSetType));
        return values[new System.Random().Next(0,values.Length-1)];
    }

#if UNITY_EDITOR
    [ContextMenu("Show GridInfos")]
    public void ShowGridInfos()
    {
        // gridMoveSets = new MoveSetType[numRow,numCol];
        // stretchedGridMoveSets = new MoveSetType[numRow * numCol];
        // RandomizeGridInfo();
        // AssignInitialObstacle();
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
        for (int h = 0; h < numRow; h++) {
            for (int w = 0; w < numCol; w++) {
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


