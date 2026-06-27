using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BeeNest", menuName = "BoardTiles/BeeNest")]
public class BeeNestSO : BoardTileSO
{
    [Header("Bee Nest Settings")]
    public EnemySpawnableSO enemyToSummon;
    public int enemyCount = 3;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        if (enemyToSummon == null) return;

        List<CellView> emptyCells = new();

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                CellView candidate = board.GetCellView(cell.x + dx, cell.y + dy);
                if (candidate == null) continue;
                if (candidate.Revealed) continue;
                if (candidate.spawnable != null) continue;
                if (candidate.boardTile != null) continue;
                if (candidate.isVoid) continue;

                emptyCells.Add(candidate);
            }
        }

        int toSummon = Mathf.Max(enemyCount, emptyCells.Count);
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
    }
}