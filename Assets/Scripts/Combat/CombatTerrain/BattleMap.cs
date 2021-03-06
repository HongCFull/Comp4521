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
    public override bool Equals(object obj) {
        if(obj is GridCoordinate){
            GridCoordinate other = (GridCoordinate) obj;
            return (other.row==this.row) && (other.col == this.col);
        }
        return false;
    }

    public bool Equals(GridCoordinate other) => (other.row == this.row) && (other.col == this.col);
    
    public override string ToString() => string.Format("row = {0}, col = {1}",row,col);

    public override int GetHashCode() => row ^ col;

}

public class BattleMap : MonoBehaviour
{
    [Header("Grid Setting")]
    [SerializeField] private Grid gridInfo;
    [SerializeField] private Transform topLeftTilePivot;
    [SerializeField] private List<GridCoordinate> initObstacles;
    [SerializeField] private Color gridMovementHighlight;
    [SerializeField] private Color gridAttackableHighlight;
    [SerializeField] private Color gridAttackConfirmHighlight;
    [SerializeField] private Color gridCasterHighlight;
    
    [Tooltip("The maximum number of tiles in the (X-axis)")]
    [SerializeField] private int numCol;
    [Tooltip("The maximum number of tiles in the (Z-axis)")]
    [SerializeField] private int numRow;
    
    [Header("Distribution settings")] 
    [SerializeField] private int meleeSkillWeight;
    [SerializeField] private int directionalSkillWeight;
    [SerializeField] private int AOESkillWeight;
    [SerializeField] private int ultimateSkillWeight;
    [SerializeField] private int defenseWeight;
    [SerializeField] private int healWeight;

    [Header("References")]
    [SerializeField] BattleTerrainCanvas battleTerrainCanvas;
    
   // public static BattleMap Instance { get; private set; }
    
    public MoveSetType[,] gridMoveSets { get; private set; }
    public Dictionary<GridCoordinate, TurnBasedActor> actorsCoord;

    private MoveSetType[] stretchedGridMoveSets;
    
    private List<GridCoordinate> walkableGridsToHighlight;
    private List<GridCoordinate> potentialCastGridsToHighlight;
    private List<GridCoordinate> confirmingGridsToHighlight;

    private float cellWidth;    //x
    private float cellHeight;   //y
    private float cellLength;   //z
    
    void Awake()
    {
     //   if (!Instance)
    //        Instance = this;
        
        gridMoveSets = new MoveSetType[numRow,numCol];
        stretchedGridMoveSets = new MoveSetType[numRow * numCol];
        actorsCoord = new Dictionary<GridCoordinate, TurnBasedActor>();
        
        walkableGridsToHighlight = new List<GridCoordinate>();
        potentialCastGridsToHighlight = new List<GridCoordinate>();
        confirmingGridsToHighlight = new List<GridCoordinate>();
        
        cellWidth = gridInfo.cellSize.x;
        cellHeight = gridInfo.cellSize.y;
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

    public int GetNumOfRow() => numRow;
    public int GetNumOfCol() => numCol;
    
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
        //Debug.Log("Rerolled "+coord+" to "+moveSetType);
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
        //Debug.Log("Get ???" +rowIndex+","+colIndex);
        return new GridCoordinate(rowIndex, colIndex);
    }

    public List<GridCoordinate> GetWalkableGridsInRange(GridCoordinate center,int range)
    {
        List<GridCoordinate> walkableGrids = new List<GridCoordinate>();
        for(int rowOffset = -range; rowOffset <= range ; rowOffset++){
            for(int colOffset = -range; colOffset<=range ; colOffset++){
                
                int rowIndex = center.row + rowOffset;
                if(rowIndex < 0 || rowIndex >= numRow)
                    break;
                
                int colIndex = center.col + colOffset;
                if(colIndex <0 || colIndex >= numCol)
                    continue;

                int manhattanDist = Mathf.Abs(rowIndex-center.row) + Mathf.Abs(colIndex-center.col);
                if(manhattanDist>range)
                    continue;
                
                GridCoordinate thisGrid = new GridCoordinate(rowIndex,colIndex);
                if(gridMoveSets[thisGrid.row,thisGrid.col]==MoveSetType.Obstacle)
                    continue;
                
                walkableGrids.Add(thisGrid);
            }
        }
        return walkableGrids;
    }
    public void RegisterTurnBasedActorOnGrid(TurnBasedActor actor, GridCoordinate coordinate)
    {
        if (actorsCoord.ContainsKey(coordinate)) {
            //Debug.Log("coordinate: "+coordinate+" is occupied by actor: " + actorsCoord[coordinate].gameObject.name);
            return;
        }
        actorsCoord.Add(coordinate,actor);
        //Debug.Log("Save actor : "+actor.gameObject.name+" in coord "+coordinate);
    }
    
    public void UnregisterActorOnGrid(TurnBasedActor actor, GridCoordinate coordinate)
    {
        if (!actorsCoord.ContainsKey(coordinate)) {
            //Debug.Log("coordinate: "+coordinate+" is empty, cant unregister actor on it");
            return;
        }
        actorsCoord.Remove(coordinate);
        //Debug.Log("Unregistered actor : "+actor.gameObject.name+" in coord "+coordinate);
    }
    
    public void DealDamageToGrids(List<GridCoordinate> gridCoordinates, float damage, bool canDmgFriendlyTarget=false)
    {
        foreach (var gridCoord in gridCoordinates) {
            if (!actorsCoord.ContainsKey(gridCoord)) 
                continue;
            if (actorsCoord[gridCoord] is IDamageable damageable) {
                
                damageable.OnDamageTaken(damage);
                Debug.Log("damaging grid "+gridCoord+" with dmg :" +damage);
            }

        }
    }
    
    public void DealDamageToFriendlyGrids(List<GridCoordinate> gridCoordinates, float damage, bool canDmgFriendlyTarget=false)
    {
        foreach (var gridCoord in gridCoordinates) {
            if (!actorsCoord.ContainsKey(gridCoord)) 
                continue;
            if (actorsCoord[gridCoord] is IDamageable damageable) {
                // if(actorsCoord[gridCoord].turnBasedActorType == TurnBasedActorType.EnemyMonster)
                //     continue;
                damageable.OnDamageTaken(damage);
                Debug.Log("damaging grid "+gridCoord+" with dmg :" +damage);
            }
        }
    }
//===========================================================================================================================    
//Highlights:
//===========================================================================================================================
    //Walkable Grids
    public void HighlightWalkableGridsInRange(GridCoordinate center, int range){
        //Debug.Log("BFS at "+center);
        walkableGridsToHighlight.Clear();
        //UpdateGridsToHighlightByBFS(center, range);
        UpdateGridsToHighlight(center,range);
        battleTerrainCanvas.HighlightGrids(walkableGridsToHighlight, gridMovementHighlight);
    }
    public void UnHighlightWalkableGrids(){
        battleTerrainCanvas.UnHighlightGrids(walkableGridsToHighlight);
        walkableGridsToHighlight.Clear();
    }

    //Potential Casting Grids
    public void HighlightPotentialCastingGrid(List<GridCoordinate> potentialCastingGrids) {
        Debug.Log("Highlight potential casting grid");
        potentialCastGridsToHighlight.Clear();
        potentialCastGridsToHighlight = new List<GridCoordinate>(potentialCastingGrids);
        battleTerrainCanvas.HighlightGrids(potentialCastGridsToHighlight, gridAttackableHighlight);
    }
    public void UnHighlightPotentialCastingGrids(){
        battleTerrainCanvas.UnHighlightGrids(potentialCastGridsToHighlight);
        potentialCastGridsToHighlight.Clear();
    }
    
    //Confirming Grids
    public void HighlightConfirmingGrids(List<GridCoordinate> confirmingGrids) {
        confirmingGridsToHighlight.Clear();
        confirmingGridsToHighlight = new List<GridCoordinate>(confirmingGrids);
        battleTerrainCanvas.HighlightGrids(confirmingGridsToHighlight, gridAttackConfirmHighlight);
    }
    public void UnHighlightConfirmingGrids(){
        battleTerrainCanvas.UnHighlightGrids(confirmingGridsToHighlight);
        confirmingGridsToHighlight.Clear();
    }
    
    public void HighlightSkillTargetGrids(List<GridCoordinate> grids) {
        battleTerrainCanvas.HighlightGrids(grids, gridAttackConfirmHighlight);
    }

    public void HighlightCasterGrid(GridCoordinate grid) {
        battleTerrainCanvas.HighlightGrid(grid, gridCasterHighlight);
    }

    public void UnhighlightGrid(GridCoordinate grid) {
        battleTerrainCanvas.UnHighlightGrid(grid);
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
        int totalWeightSum = meleeSkillWeight + directionalSkillWeight + AOESkillWeight + ultimateSkillWeight + defenseWeight + healWeight;
        int numOfSkillA = numRow * numCol* meleeSkillWeight / totalWeightSum;
        int numOfSkillB = numRow * numCol* directionalSkillWeight / totalWeightSum;
        int numOfSkillC = numRow * numCol* AOESkillWeight / totalWeightSum;
        int numOfSkillD = numRow * numCol* ultimateSkillWeight / totalWeightSum;
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
            gridMoveSets[coord.row, coord.col] = MoveSetType.Obstacle;
        }
    }

    void InitializeMoveSetUIOnGrids() => battleTerrainCanvas.InitializeTileImages(gridMoveSets,numRow,numCol);

    //Get a random GridMoveSet except Obstacle
    MoveSetType GetRandomGridMoveSet()
    {
        MoveSetType[] values = (MoveSetType[]) Enum.GetValues(typeof(MoveSetType));
        return values[new System.Random().Next(0,values.Length-2)]; //NO obstacles and pickup
    }

    void UpdateGridsToHighlight(GridCoordinate start,int range){
        for(int rowOffset = -range; rowOffset <= range ; rowOffset++){
            for(int colOffset = -range; colOffset<=range ; colOffset++){
                
                int rowIndex = start.row + rowOffset;
                if(rowIndex < 0 || rowIndex >= numRow)
                    break;
                
                int colIndex = start.col + colOffset;
                if(colIndex <0 || colIndex >= numCol)
                    continue;

                int manhattanDist = Mathf.Abs(rowIndex-start.row) + Mathf.Abs(colIndex-start.col);
                if(manhattanDist<=range){
                    GridCoordinate thisGrid = new GridCoordinate(rowIndex,colIndex);
                    walkableGridsToHighlight.Add(thisGrid);
                } 
            }
        }
    }

#if UNITY_EDITOR
    
    [ContextMenu("Deal 50 dmg to all grids")]
    public void DealDamageToWholeMap()
    {
        foreach (var pair in actorsCoord) {
            Debug.Log(pair.Key+" has "+pair.Value.gameObject.name);
        }
        
        GridCoordinate currentGrid = new GridCoordinate(0, 0);
        for (int r = 0; r < numRow; r++) {
            for (int c = 0; c < numCol; c++) {
                currentGrid.row = r;
                currentGrid.col = c;
                
                if (!actorsCoord.ContainsKey(currentGrid)) 
                    continue;     
                
                if (actorsCoord[currentGrid] is IDamageable damageable) {
                    damageable.OnDamageTaken(50f);
                    Debug.Log("dealing 50 damage to "+actorsCoord[currentGrid].gameObject.name);
                }
            }
        }
        
    }
    
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


