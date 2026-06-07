using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FakeMineAbility", menuName = "Minesweeper/Abilities/FakeMineAbility")]
public class FakeMineSO : SpawnableAbilitiesSO, ISpawnableOnBoard
{
    public int displayDmg = 1001;
    public int actualDmg = 1;

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        revealedCell.damageOverride = actualDmg;

        board.RefreshAllCellDamageValues();

    }

    public void OnBoardSpawn(CellView sourceCell, BoardManager board)
    {
        sourceCell.damageOverride = displayDmg;
        board.RefreshAllCellDamageValues();
    }
}
