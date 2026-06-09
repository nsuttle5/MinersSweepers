using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ObscureAttackValuesAbility", menuName = "Minesweeper/Abilities/ObscureAttackValues")]
public class ObscureAttackValuesAbilitySO : SpawnableAbilitiesSO, ISpawnableOnBoard
{
    public int size = 3;
    public bool[] obscurePattern;

    private void OnEnable()
    {
        if (obscurePattern == null || obscurePattern.Length != size * size)
            obscurePattern = new bool[size * size];
    }

    public void OnBoardSpawn(CellView sourceCell, BoardManager board)
    {
        ApplyObscure(sourceCell, board, true);
    }

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        ApplyObscure(revealedCell, board, false);
        board.RefreshAllCellDamageValues();
    }

    private void ApplyObscure(CellView sourceCell, BoardManager board, bool obscure)
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

                if (index < 0 || index >= obscurePattern.Length) continue;
                if (!obscurePattern[index]) continue;

                CellView target = board.GetCellView(cx + dx, cy + dy);
                if (target == null || target == sourceCell) continue;

                target.isDamageObscured = obscure;
                target.TryDisplaySurroundingDamage();
            }
        }
    }

    public void RevertObscure(CellView sourceCell, BoardManager board)
    {
        ApplyObscure(sourceCell, board, false);
        board.RefreshAllCellDamageValues();
    }
}