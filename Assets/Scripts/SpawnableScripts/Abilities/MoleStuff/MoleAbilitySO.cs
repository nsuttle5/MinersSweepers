using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MoleAbility", menuName = "Minesweeper/Abilities/MoleAbility")]
public class MoleAbilitySO : SpawnableAbilitiesSO
{
    public MoleHoleSpawnableSO moleHoleType;

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        List<CellView> allCells = new List<CellView>();
        allCells.AddRange(board.unrevealedCells);
        allCells.AddRange(board.revealedCells);

        List<CellView> otherHoles = new List<CellView>();
        foreach (var cell in allCells)
        {
            if (cell == revealedCell) continue;
            if (cell.State == CellState.Interacted) continue;
            if (cell.State == CellState.Cleared) continue;
            if (cell.spawnable is MoleHoleSpawnableSO)
                otherHoles.Add(cell);
        }

        if (otherHoles.Count == 0)
            return;

        List<CellView> unrevealedHoles = otherHoles.FindAll(c => !c.Revealed);
        CellView targetHole = unrevealedHoles.Count > 0
            ? unrevealedHoles[Random.Range(0, unrevealedHoles.Count)]
            : otherHoles[Random.Range(0, otherHoles.Count)];

        SpawnableSO mole = revealedCell.spawnable;

        targetHole.spawnable = mole;
        BoardSidebarTracker.Instance?.RemoveSpawnable(moleHoleType);

        if (!targetHole.Revealed)
        {
            targetHole.SetState(CellState.Hidden);
            board.NotifyCellHidden(targetHole);
        }
        else
        {
            targetHole.UpdateVisual();
        }

        revealedCell.spawnable = moleHoleType;
        revealedCell.UpdateVisual();

        board.RefreshAllCellDamageValues();
    }
}