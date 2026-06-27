using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BeeNest", menuName = "BoardTiles/BeeNest")]
public class BeeNestSO : BoardTileSO
{
    [Header("Bee Nest Settings")]
    public EnemySpawnableSO enemyToSummon;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        BeeSwarmSpawn(cell, board);
    }

    public void BeeSwarmSpawn(CellView cell, BoardManager board)
    {
        /*
        // Check surrounding tiles of the cell to see if they are empty, if they are empty, spawn the enemy, refresh the board, use CellView list for empty cells
        List<CellView> emptyCells = new();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;
                CellView candidate = board.GetCellView(cell.X + x, cell.Y + y);
                if (candidate == null) continue;
                if (candidate.Revealed) continue;
                if (candidate.spawnable != null) continue;
                if (candidate.boardTile != null) continue;
                if (candidate.isVoid) continue;

                emptyCells.Add(candidate);
            }
        }

        int toSummon = Mathf.Min(enemyCount, emptyCells.Count);
        for (int i = 0; i < toSummon; i++)
        {
            int index = Random.Range(0, emptyCells.Count);
            CellView target = emptyCells[index];
            emptyCells.RemoveAt(index);

            target.spawnable = enemyToSummon;
            target.UpdateVisual();

            BoardSidebarTracker.Instance?.AddSpawnable(enemyToSummon);
        }

        board.RefreshAllCellDamageValues();
        */
    }

}
