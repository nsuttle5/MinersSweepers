using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[CreateAssetMenu(fileName = "CaveQuakeTile", menuName = "BoardTiles/CaveQuakeTile")]
public class CaveQuakeTileSO : BoardTileSO
{
    [Header("Shimmy")]
    public float shimmyAmount = 0.08f;
    public float shimmySpeed = 4f;

    public override void OnBoardSpawn(CellView cell, BoardManager board)
    {
        board.StartCoroutine(ShimmyWhileHidden(cell));
    }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        ShuffleUnrevealedTiles(cell, board);
    }

    private IEnumerator ShimmyWhileHidden(CellView cell)
    {
        Vector3 originalLocalPos = cell.transform.localPosition;
        float t = 0f;

        while (cell != null && cell.boardTile == this && !cell.Revealed)
        {
            t += Time.deltaTime * shimmySpeed;
            float offsetX = Mathf.Sin(t) * shimmyAmount;

            cell.transform.localPosition = originalLocalPos + new Vector3(offsetX, 0f, 0f);

            yield return null;
        }

        if (cell != null)
            cell.transform.localPosition = originalLocalPos;
    }

    private void ShuffleUnrevealedTiles(CellView sourceCell, BoardManager board)
    {
        List<CellView> unrevealedCells = new();

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                CellView candidate = board.GetCellView(x, y);
                if (candidate == null) continue;
                if (candidate.Revealed) continue;
                if (candidate.isVoid) continue;
                if (candidate == sourceCell) continue;

                unrevealedCells.Add(candidate);
            }
        }

        List<SpawnableSO> spawnablesToShuffle = new();
        List<BoardTileSO> boardTilesToShuffle = new();

        foreach (var c in unrevealedCells)
        {
            spawnablesToShuffle.Add(c.spawnable);
            boardTilesToShuffle.Add(c.boardTile);
        }

        for (int i = spawnablesToShuffle.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (spawnablesToShuffle[i], spawnablesToShuffle[j]) = (spawnablesToShuffle[j], spawnablesToShuffle[i]);
            (boardTilesToShuffle[i], boardTilesToShuffle[j]) = (boardTilesToShuffle[j], boardTilesToShuffle[i]);
        }

        for (int i = 0; i < unrevealedCells.Count; i++)
        {
            CellView c = unrevealedCells[i];

            c.spawnable = spawnablesToShuffle[i];
            c.boardTile = boardTilesToShuffle[i];

            c.Mark(string.Empty);
            c.UpdateVisual();

            if (c.boardTile == this)
                board.StartCoroutine(ShimmyWhileHidden(c));
        }

        board.RefreshAllCellDamageValues();
    }
}
