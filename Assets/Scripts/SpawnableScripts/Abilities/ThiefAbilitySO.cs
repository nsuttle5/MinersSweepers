using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ThiefAbility", menuName = "Minesweeper/Abilities/ThiefAbility")]
public class ThiefAbilitySO : SpawnableAbilitiesSO
{
    public int goldToSteal = 1;

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        if (revealedCell.spawnable == null) return;
        //if (revealedCell.spawnable.type != SpawnableType.Gold) return;


        PlayerProfileManager.Instance.AddGoldToWallet(-goldToSteal);
        Debug.Log($"Player gold after stealing: {PlayerProfileManager.Instance.TotalGold}");
        //revealedCell.spawnable.health -= goldToSteal;
        if (revealedCell.spawnable.health <= 0)
        {
            revealedCell.spawnable = null;
        }
        revealedCell.UpdateVisual();
    }
}
