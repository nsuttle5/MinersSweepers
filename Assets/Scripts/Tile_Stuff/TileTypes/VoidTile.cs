using UnityEngine;

[CreateAssetMenu(fileName = "VoidTile", menuName = "BoardTiles/Void Tile")]
public class VoidTile : BoardTileSO
{
    public override void OnBoardSpawn(CellView cell, BoardManager boardManager)
    {
        cell.spawnable = null;
        cell.SetVoid(true);
    }

    public override void OnReveal(CellView cell, BoardManager boardManager)
    {

    }
}
