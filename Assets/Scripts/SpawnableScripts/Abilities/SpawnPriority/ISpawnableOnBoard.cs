using UnityEngine;

public interface ISpawnableOnBoard
{
    void OnBoardSpawn(CellView sourceCell, BoardManager board);
}
