using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FakeMineAbility", menuName = "Minesweeper/Abilities/FakeMineAbility")]
public class FakeMineSO : SpawnableAbilitiesSO
{
    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        if (revealedCell.spawnable == null) return;
        if (revealedCell.spawnable.type != SpawnableType.Enemy) return;

        revealedCell.spawnable.damage = 1;
        revealedCell.UpdateVisual();

    }
}
