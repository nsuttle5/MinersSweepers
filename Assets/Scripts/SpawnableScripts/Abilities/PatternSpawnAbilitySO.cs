using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatternSpawnAbility", menuName = "Minesweeper/Abilities/PatternSpawn")]
public class PatternSpawnAbilitySO : SpawnableAbilitiesSO, ISpawnableOnBoard
{
    public int size = 3;

    [System.Serializable]
    public class PatternCell
    {
        public bool active;
        public SpawnableSO spawnableToPlace;
    }

    public PatternCell[] spawnPattern = new PatternCell[9];

    public void OnBoardSpawn(CellView sourceCell, BoardManager board)
    {
        PlacePattern(sourceCell, board);
    }

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {

    }

    private void PlacePattern(CellView sourceCell, BoardManager board)
    {
        int cx = sourceCell.x;
        int cy = sourceCell.y;
        int half = size / 2;

        for (int dx = -half; dx <= half; dx++)
        {
            for (int dy = -half; dy <= half; dy++)
            {
                int px = dx + half;
                int py = dy + half;
                int index = px + py * size;

                if (index < 0 || index >= spawnPattern.Length) continue;

                PatternCell patternCell = spawnPattern[index];
                if (!patternCell.active || patternCell.spawnableToPlace == null) continue;

                int nx = cx + dx;
                int ny = cy + dy;
                CellView targetCell = board.GetCellView(nx, ny);

                if (targetCell == null) continue;
                if (targetCell == sourceCell) continue;
                if (targetCell.spawnable != null)
                {
                    targetCell = FindNearestEmptyCell(board, nx, ny, cx, cy);
                    if (targetCell == null) continue;
                }

                targetCell.spawnable = patternCell.spawnableToPlace;
                targetCell.UpdateVisual();
            }
        }

        board.RefreshAllCellDamageValues();
    }

    private CellView FindNearestEmptyCell(BoardManager board, int preferX, int preferY, int originX, int originY)
    {
        CellView best = null;
        float bestDist = float.MaxValue;

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                if (x == originX && y == originY) continue;
                CellView candidate = board.GetCellView(x, y);
                if (candidate == null || candidate.spawnable != null) continue;

                float dist = UnityEngine.Vector2.Distance(
                    new UnityEngine.Vector2(preferX, preferY),
                    new UnityEngine.Vector2(x, y));

                if (dist < bestDist)
                {
                    bestDist = dist;
                    best = candidate;
                }
            }
        }

        return best;
    }
}