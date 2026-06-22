using UnityEngine;

public enum DamageModifyOperation { Add, Subtract, Multiply, Divide }

[CreateAssetMenu(fileName = "PatternModifyDamageAbility", menuName = "Minesweeper/Abilities/PatternModifyDamage")]
public class PatternModifyDamageAbilitySO : SpawnableAbilitiesSO, ISpawnableOnBoard
{
    public int size = 3;

    [System.Serializable]
    public class ModifyCell
    {
        public bool active;
        public DamageModifyOperation operation;
        public int value = 1;
    }

    public ModifyCell[] modifyPattern;

    private void OnEnable()
    {
        if (modifyPattern == null || modifyPattern.Length != size * size)
        {
            modifyPattern = new ModifyCell[size * size];
            for (int i = 0; i < modifyPattern.Length; i++)
                modifyPattern[i] = new ModifyCell();
        }
    }

    public void OnBoardSpawn(CellView sourceCell, BoardManager board)
    {
        ApplyModifications(sourceCell, board);
    }

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        ApplyModifications(revealedCell, board);
    }

    public void RevertModifications(CellView sourceCell, BoardManager board)
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

                if (index < 0 || index >= modifyPattern.Length) continue;
                if (!modifyPattern[index].active) continue;

                CellView target = board.GetCellView(cx + dx, cy + dy);
                if (target == null || target == sourceCell) continue;

                target.damageOverride = null;
            }
        }

        board.RefreshAllCellDamageValues();
    }

    private void ApplyModifications(CellView sourceCell, BoardManager board)
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

                if (index < 0 || index >= modifyPattern.Length) continue;

                ModifyCell modCell = modifyPattern[index];
                if (!modCell.active) continue;

                CellView target = board.GetCellView(cx + dx, cy + dy);
                if (target == null || target == sourceCell) continue;
                if (target.spawnable == null || target.spawnable.damage <= 0) continue;

                int current = target.EffectiveDamage;
                int newDamage = modCell.operation switch
                {
                    DamageModifyOperation.Add => current + modCell.value,
                    DamageModifyOperation.Subtract => Mathf.Max(0, current - modCell.value),
                    DamageModifyOperation.Multiply => current * modCell.value,
                    DamageModifyOperation.Divide => modCell.value != 0 ? current / modCell.value : current,
                    _ => current
                };

                target.damageOverride = newDamage;
            }
        }

        board.RefreshAllCellDamageValues();
    }
}