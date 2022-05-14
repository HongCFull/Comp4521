using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.AI;

public enum GridInfo
{
    SkillA,
    SkillB,
    SkillC,
    SkillD,
    Defense,
    Heal,
    PickUp,
    Obstacle
}

[System.Serializable]
[Tooltip("Row Col = 0,0 at the top left corner")]
public struct TileCoordinate
{
    public int row;
    public int col;
}

public class BattleTerrain : MonoBehaviour
{
    [SerializeField] private Grid gridInfo;
    [SerializeField] private Transform topLeftTilePivot;
    
    [Tooltip("The maximum number of tiles in the (X-axis)")]
    [SerializeField] private int width;
    [Tooltip("The maximum number of tiles in the (Z-axis)")]
    [SerializeField] private int height;
    [SerializeField] private List<TileCoordinate> initObstacles;
    
    [Header("Distribution settings")] 
    [SerializeField] private int skillAWeight;
    [SerializeField] private int skillBWeight;
    [SerializeField] private int skillCWeight;
    [SerializeField] private int skillDWeight;
    [SerializeField] private int defenseWeight;
    [SerializeField] private int healWeight;

    public GridInfo[,] gridInfos { get; private set; }
    
    private GridInfo[] stretchedGridInfo;
    private float cellWidth;
    private float cellHeight;
    void Awake()
    {
        gridInfos = new GridInfo[height,width];
        stretchedGridInfo = new GridInfo[height * width];

        cellWidth = gridInfo.cellSize.x;
        Debug.Log("gridinfo cell size z = "+gridInfo.cellSize.z);
        cellHeight = gridInfo.cellSize.z;
        
        RandomizeGridInfo();
        AssignInitialObstacle();
    }

    /// <summary>
    /// Return a tuple of row index and the column index 
    /// </summary>
    public Tuple<int, int> GetGridIndexByWorldPos(Vector3 pos)
    {
        int rowIndex = (int) Math.Round((pos.z - topLeftTilePivot.position.z) / cellHeight);
        int colIndex = (int) Math.Round((pos.x - topLeftTilePivot.position.x) / cellWidth);
        //Debug.Log("Get ：" +rowIndex+","+colIndex);
        return new Tuple<int, int>(rowIndex, colIndex);
    }

    public Vector3 GetWalkableGridCenterByWorldPos(Vector3 pos)
    {
        Tuple<int, int> closestGridIndex = GetGridIndexByWorldPos(pos);
        NavMeshHit hit;
        Vector3 gridCenter = new Vector3(topLeftTilePivot.position.x + closestGridIndex.Item2 * cellWidth, pos.y, topLeftTilePivot.position.z+closestGridIndex.Item1 * cellHeight);
        //return gridCenter;
        //Debug.Log("Grid center = "+gridCenter);
        NavMesh.SamplePosition(gridCenter, out hit, 5f,1);
        //Debug.Log("Hit position " +hit.position);
        return hit.position;
    }

    void AssignInitialObstacle()
    {
        foreach (TileCoordinate coord in initObstacles) {
            gridInfos[coord.row, coord.col] = GridInfo.Obstacle;
        }
    }

    private void RandomizeGridInfo()
    {
        InitStretchedGridInfo();
        RandomizeStretchedGridInfo();
        for (int h = 0; h < height; h++) {
            for (int w = 0; w < width; w++) {
                gridInfos[h, w] = stretchedGridInfo[w * h + h];
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
                stretchedGridInfo[i] = GridInfo.SkillA;
                numOfSkillA--;
            }else if (numOfSkillB > 0) {
                stretchedGridInfo[i] = GridInfo.SkillB;
                numOfSkillB--;
            }else if (numOfSkillC > 0) {
                stretchedGridInfo[i] = GridInfo.SkillC;
                numOfSkillC--;
            }else if (numOfSkillD > 0) {
                stretchedGridInfo[i] = GridInfo.SkillD;
                numOfSkillD--;
            }else if (numOfDefense > 0) {
                stretchedGridInfo[i] = GridInfo.Defense;
                numOfDefense--;
            }else if (numOfHeal > 0) {
                stretchedGridInfo[i] = GridInfo.Heal;
                numOfHeal--;
            }else {
                stretchedGridInfo[i] = GridInfo.SkillA;
            }
        }
    }
    private void RandomizeStretchedGridInfo() {
        var rng = new System.Random();
        rng.Shuffle(stretchedGridInfo);
    }


#if UNITY_EDITOR
    [ContextMenu("Show GridInfos")]
    public void ShowGridInfos()
    {
        gridInfos = new GridInfo[height,width];
        stretchedGridInfo = new GridInfo[height * width];
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

                if (gridInfos!=null) {
                    tileWorldPos.z += 0.5f;
                    Handles.Label(tileWorldPos,gridInfos[h,w].ToString());
                }

            }
        }
    }
#endif
}


