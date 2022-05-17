using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleTerrainCanvas : MonoBehaviour
{
    [SerializeField] private BattleTerrain battleTerrain;
    [SerializeField] private MoveSetOnGrid moveSetOnGridSetting;
    
    [Tooltip("Indexed 0 at the top left corner, increased by col and then row")]
    [SerializeField] List<Image> tileImages;

    [Tooltip("Default transparency of the tile iamge")]
    [SerializeField] [Range(0,255)] int defaultTransaprency;

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
                tileImages[coord.row*numOfCols + coord.col].color=new Color(1,1,1,0);
            else
                tileImages[coord.row*numOfCols + coord.col].color=new Color(1,1,1,defaultTransaprency/255f);
    }

    public void UpdateMoveSetImageAtGridTo(int row, int col,MoveSetOnGrid.MoveSetType moveSetType)
    {
        Sprite sprite = moveSetOnGridSetting.GetSpriteByMoveSetType(moveSetType);
        tileImages[row*numOfCols + col].sprite = sprite;
                
            if(!sprite)
                tileImages[row*numOfCols + col].color=new Color(1,1,1,0);
            else
                tileImages[row*numOfCols + col].color=new Color(1,1,1,defaultTransaprency/255f);
    }
    
}
