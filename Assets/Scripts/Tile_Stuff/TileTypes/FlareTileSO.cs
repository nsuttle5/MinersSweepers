using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FlareTile", menuName = "BoardTiles/FlareTile")]
public class FlareTileSO : BoardTileSO
{
    [Header("Radius")]
    public int radius = 1;

    [Header("Pulse")]
    public Color pulseColor = Color.yellow;
    public float pulseSpeed = 2f;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        CellView exitCell = FindExitCell(board);
        if (exitCell == null) return;

        board.StartCoroutine(PulseAroundExit(exitCell, board));
    }

    private CellView FindExitCell(BoardManager board)
    {
        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                CellView candidate = board.GetCellView(x, y);
                if (candidate != null && candidate.spawnable is ExitSpawnableSO)
                    return candidate;
            }
        }
        return null;
    }

    private IEnumerator PulseAroundExit(CellView exitCell, BoardManager board)
    {
        float t = 0f;

        while (true)
        {
            if (exitCell.Revealed)
            {
                for (int dx = -radius; dx <= radius; dx++)
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        CellView c = board.GetCellView(exitCell.x + dx, exitCell.y + dy);
                        if (c != null) c.SetTintOverride(Color.white);
                    }
                yield break;
            }

            List<CellView> targets = new();
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    CellView target = board.GetCellView(exitCell.x + dx, exitCell.y + dy);
                    if (target != null && !target.Revealed)
                        targets.Add(target);
                }
            }

            t += Time.deltaTime * pulseSpeed;
            float blend = (Mathf.Sin(t) + 1f) * 0.5f;

            Color currentColor = Color.Lerp(Color.white, pulseColor, blend);

            foreach (var target in targets)
                target.SetTintOverride(currentColor);

            yield return null;
        }
    }
}