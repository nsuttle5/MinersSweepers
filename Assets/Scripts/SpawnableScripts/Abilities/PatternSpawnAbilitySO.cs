using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PatternSpawnAbility", menuName = "Minesweeper/Abilities/PatternSpawn")]
public class PatternSpawnAbilitySO : SpawnableAbilitiesSO
{
    public int size = 3;

    [System.Serializable]
    public class PatternCell
    {
        public bool active;
        public SpawnableSO spawnableToPlace;
    }

    public PatternCell[] spawnPattern = new PatternCell[9];

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        int cx = revealedCell.x;
        int cy = revealedCell.y;
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
                if (targetCell == revealedCell) continue;
                if (targetCell.State == CellState.Interacted) continue;
                if (targetCell.State == CellState.Cleared) continue;
                if (targetCell.spawnable != null) continue;

                targetCell.spawnable = patternCell.spawnableToPlace;
                targetCell.UpdateVisual();
            }
        }

        board.RefreshAllCellDamageValues();
    }
}