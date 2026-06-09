using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BlockExitCell", menuName = "Minesweeper/Abilities/BlockExitCell")]
public class BlockExitCellSO : SpawnableAbilitiesSO
{
    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        if (revealedCell.spawnable == null) return;
        if (revealedCell.spawnable.type != SpawnableType.Exit) return;

        revealedCell.spawnable.health = int.MaxValue;
        revealedCell.UpdateVisual();
    }
}
