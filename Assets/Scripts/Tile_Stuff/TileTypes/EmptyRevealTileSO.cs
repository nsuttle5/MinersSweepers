using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EmptyRevealTile", menuName = "BoardTiles/EmptyRevealTile")]
public class EmptyRevealTileSO : BoardTileSO
{
    [Header("Cascade Animation")]
    public float ringDelay = 0.05f;
    public float popScaleUp = 1.15f;
    public float popDuration = 0.12f;
    public float popSettleDuration = 0.08f;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        board.StartCoroutine(CascadeRevealAnimated(cell, board));
    }

    private IEnumerator CascadeRevealAnimated(CellView startCell, BoardManager board)
    {
        List<List<CellView>> rings = new();
        Queue<CellView> toProcess = new();
        HashSet<CellView> visited = new();

        toProcess.Enqueue(startCell);
        visited.Add(startCell);

        List<CellView> currentRing = new() { startCell };

        while (currentRing.Count > 0)
        {
            List<CellView> nextRing = new();

            foreach (var current in currentRing)
            {
                CellView[] neighbors = new CellView[]
                {
                    board.GetCellView(current.x, current.y - 1),
                    board.GetCellView(current.x, current.y + 1),
                    board.GetCellView(current.x - 1, current.y),
                    board.GetCellView(current.x + 1, current.y),
                };

                foreach (var neighbor in neighbors)
                {
                    if (neighbor == null) continue;
                    if (visited.Contains(neighbor)) continue;
                    if (neighbor.isVoid) continue;
                    if (neighbor.Revealed) continue;
                    if (neighbor.spawnable != null) continue;

                    visited.Add(neighbor);
                    nextRing.Add(neighbor);
                }
            }

            if (nextRing.Count > 0)
                rings.Add(nextRing);

            currentRing = nextRing;
        }

        foreach (var ring in rings)
        {
            foreach (var cell in ring)
            {
                cell.Reveal(wasDirectClick: false, triggerAbilities: false);
                board.OnCellRevealedNotify(cell);
                board.StartCoroutine(PopCell(cell));
            }

            yield return new WaitForSeconds(ringDelay);
        }
    }

    private IEnumerator PopCell(CellView cell)
    {
        if (cell == null) yield break;

        Vector3 normalScale = cell.transform.localScale;
        Vector3 overshootScale = normalScale * popScaleUp;

        yield return ScaleCell(cell.transform, Vector3.zero, overshootScale, popDuration);

        if (cell == null) yield break;

        yield return ScaleCell(cell.transform, overshootScale, normalScale, popSettleDuration);
    }

    private IEnumerator ScaleCell(Transform t, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;

            elapsed += Time.deltaTime;
            float e = Mathf.Clamp01(elapsed / duration);
            t.localScale = Vector3.Lerp(from, to, e);
            yield return null;
        }

        if (t != null) t.localScale = to;
    }
}