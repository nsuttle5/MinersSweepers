using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "LuckyBlockTile", menuName = "BoardTiles/LuckyBlockTile")]
public class LuckyBlockTileSO : BoardTileSO
{
    [Header("Lucky Block Settings")]
    public List<BoardTileSO> possibleTileSpawns;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        SpawnRandomTile(cell, board);
    }

    //Currently not working, only working spawn is void, but that is still bugging
    public void SpawnRandomTile(CellView cell, BoardManager board)
    {
        if (possibleTileSpawns == null || possibleTileSpawns.Count == 0) return;

        int randomIndex = Random.Range(0, possibleTileSpawns.Count);
        BoardTileSO tileToSpawn = possibleTileSpawns[randomIndex];
        Debug.Log($"Spawning {tileToSpawn.name} at {cell.transform.position}");

        cell.boardTile = tileToSpawn;
        cell.UpdateVisual();
        tileToSpawn.OnBoardSpawn(cell, board);

        board.RefreshAllCellDamageValues();
    }

}
