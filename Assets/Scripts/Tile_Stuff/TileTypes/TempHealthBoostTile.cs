using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "TempHealthBoostTile", menuName = "BoardTiles/TempHealthBoostTile")]
public class TempHealthBoostTile : BoardTileSO
{
    [Header("Temp Health Boost Settings")]
    public int healthBoost = 5;

    public int revealsUntilRevert = 3;
    public bool revertOnReveal = true;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }


    public override void OnReveal(CellView cell, BoardManager board)
    {

    }

}
