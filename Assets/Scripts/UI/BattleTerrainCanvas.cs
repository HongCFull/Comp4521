using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTerrainCanvas : MonoBehaviour
{
    [SerializeField] private BattleMap battleMap;
    [SerializeField] private MoveSetOnGrid moveSetOnGridSetting;
    
    [Tooltip("Indexed 0 at the top left corner, increased by col and then row")]
    [SerializeField] List<Image> tileImages;

    // [SerializeField] Color gridHighlightColor;
    [SerializeField] Color gridDefaultColor;

    Color invisibleGridColor = new Color(1f,1f,1f,0f);
    private int numOfRows;
    private int numOfCols;

    public void InitializeTileImages(MoveSetOnGrid.MoveSetType[,] gridMoveSets,int numOfRows,int numOfCols)
    {
        if(numOfRows*numOfCols!=tileImages.Count){
            throw new System.Exception("Grid UI count and GridMoveSets array aren't in the same size!");
        }

        this.numOfRows = numOfRows;
        this.numOfCols = numOfCols;

        for(int row=0; row < numOfRows ; row++){
            for (int col = 0; col < numOfCols; col++){
                UpdateMoveSetImageAtGridTo(row,col,gridMoveSets[row,col]);
            }
        }
    }

    public void UpdateMoveSetImageAtGridTo(GridCoordinate coord,MoveSetOnGrid.MoveSetType moveSetType)
    {
        Sprite sprite = moveSetOnGridSetting.GetSpriteByMoveSetType(moveSetType);
        tileImages[coord.row*numOfCols + coord.col].sprite = sprite;
                
            if(!sprite)
                tileImages[coord.row*numOfCols + coord.col].color = invisibleGridColor;
            else
                tileImages[coord.row*numOfCols + coord.col].color = gridDefaultColor;
    }

    public void UpdateMoveSetImageAtGridTo(int row, int col,MoveSetOnGrid.MoveSetType moveSetType)
    {
        Sprite sprite = moveSetOnGridSetting.GetSpriteByMoveSetType(moveSetType);
        tileImages[row*numOfCols + col].sprite = sprite;
                
            if(!sprite)
                tileImages[row*numOfCols + col].color = invisibleGridColor;
            else
                tileImages[row*numOfCols + col].color = gridDefaultColor;
    }
    
    public void HighlightGrid(GridCoordinate grid, Color highlightColor)
    {
        if(!tileImages[grid.row*numOfCols + grid.col].sprite){
            tileImages[grid.row*numOfCols + grid.col].color = invisibleGridColor;
        }
        tileImages[grid.row*numOfCols + grid.col].color = highlightColor;
    }

    public void HighlightGrids(List<GridCoordinate> grids, Color highlightColor)
    {
        foreach(GridCoordinate grid in grids){
            if(!tileImages[grid.row*numOfCols + grid.col].sprite){
                tileImages[grid.row*numOfCols + grid.col].color = invisibleGridColor;
                continue;
            }
            tileImages[grid.row*numOfCols + grid.col].color = highlightColor;
        }
    }

    public void UnHighlightGrid(GridCoordinate grid) {
        if(!tileImages[grid.row*numOfCols + grid.col].sprite){
            tileImages[grid.row*numOfCols + grid.col].color = invisibleGridColor;
        }
        tileImages[grid.row*numOfCols + grid.col].color = gridDefaultColor;
    }

    public void UnHighlightGrids(List<GridCoordinate> grids){
        foreach(GridCoordinate grid in grids){
            if(!tileImages[grid.row*numOfCols + grid.col].sprite){
                tileImages[grid.row*numOfCols + grid.col].color = invisibleGridColor;
                continue;
            }
            tileImages[grid.row*numOfCols + grid.col].color = gridDefaultColor;
            
        }
    }
}
