using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RevealRandomCellAbility", menuName = "Minesweeper/Abilities/RevealRandomCell")]
public class RevealRandomCellAbilitySO : SpawnableAbilitiesSO
{
    public override void OnReveal(CellView revealedCell, BoardManager board)
    {
        var unrevealed = new List<CellView>(board.unrevealedCells);
        unrevealed.Remove(revealedCell);
        Debug.Log($"Unrevealed cell count: {unrevealed.Count}");
        if (unrevealed.Count > 0)
        {
            var cell = unrevealed[Random.Range(0, unrevealed.Count)];
            Debug.Log("Revealing cell: " + cell.name);
            cell.Reveal();
            BoardManager.OnCellRevealed?.Invoke(cell);
        }
        else
        {
            Debug.Log("No unrevealed cell left to reveal.");
        }
    }
}