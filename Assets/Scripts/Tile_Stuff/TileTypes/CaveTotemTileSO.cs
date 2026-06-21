using UnityEngine;

[CreateAssetMenu(fileName = "CaveTotemTile", menuName = "BoardTiles/CaveTotemTile")]
public class CaveTotemTileSO : BoardTileSO
{
    [Header("Trade")]
    public int goldReward = 30;
    public int damageBuffPerEnemy = 2;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        GameData.Instance.CollectGold(goldReward);
        BuffRemainingEnemies(cell, board);
    }

    private void BuffRemainingEnemies(CellView sourceCell, BoardManager board)
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                CellView candidate = board.GetCellView(x, y);
                if (candidate == null || candidate == sourceCell) continue;
                if (candidate.Revealed) continue;
                if (candidate.spawnable is not EnemySpawnableSO) continue;

                int current = candidate.effectiveDamage;
                candidate.damageOverride = current + damageBuffPerEnemy;
            }
        }

        board.RefreshAllCellDamageValues();
    }
}
