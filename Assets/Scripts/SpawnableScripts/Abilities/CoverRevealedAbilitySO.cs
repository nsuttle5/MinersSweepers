using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CoverRevealedAbility", menuName = "Minesweeper/Abilities/CoverRevealed")]
public class CoverRevealedAbilitySO : SpawnableAbilitiesSO
{
    [Range(1, 20)] public int cellsToCover = 3;
    [Range(0f, 1f)] public float spawnChance = 0.5f;
    public List<SpawnableSO> possibleSpawns;

    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        List<CellView> candidates = new(board.revealedCells);
        candidates.Remove(revealedCell);

        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int r = Random.Range(0, i + 1);
            (candidates[r], candidates[i]) = (candidates[i], candidates[r]);
        }

        int count = Mathf.Min(cellsToCover, candidates.Count);
        for (int i = 0; i < count; i++)
        {
            CellView target = candidates[i];

            if (Random.value < spawnChance && possibleSpawns.Count > 0)
                target.spawnable = possibleSpawns[Random.Range(0, possibleSpawns.Count)];

            target.SetState(CellState.Hidden);
            board.NotifyCellHidden(target);
        }

        board.RefreshAllCellDamageValues();
        board.RefreshAllCellVisuals();
    }
}