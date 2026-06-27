using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "FakeRock", menuName = "BoardTiles/FakeRock")]
public class FakeRockTileSO : BoardTileSO
{
    [Header("Fake Rock Settings")]
    public int healthCost = 1;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        PlayerRunStats.Instance?.ModifyHealth(-healthCost);
        AttackSequenceManager.Instance?.TriggerPlayerHitReaction();
    }
}
