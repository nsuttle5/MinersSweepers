using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BloodAltarTile", menuName = "BoardTiles/BloodAltarTile")]
public class BloodAltarTileSO : BoardTileSO
{
    [Header("Trade")]
    public int goldReward = 25;
    public int healthCost = 10;

    [Header("Summoned Enemies")]
    public EnemySpawnableSO enemyToSummon;
    public int enemyCount = 2;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        GameData.Instance.CollectGold(goldReward);
        PlayerRunStats.Instance?.ModifyHealth(-healthCost);

        SummonEnemies(board);
    }

    private void SummonEnemies(BoardManager board)
    {
        if (enemyToSummon == null || enemyCount <= 0) return;

        List<CellView> emptyCells = new();
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                CellView candidate = board.GetCellView(x, y);
                if (candidate == null) continue;
                if (candidate.Revealed) continue;
                if (candidate.spawnable != null) continue;
                if (candidate.boardTile != null) continue;
                //if (candidate.isVoid) continue;

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
    }
}