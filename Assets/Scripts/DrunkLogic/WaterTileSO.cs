using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "WaterTile", menuName = "BoardTiles/WaterTile")]
public class WaterTileSO : BoardTileSO
{
    [Header("Idle Ripple Animation")]
    public Sprite[] rippleFrames;
    public float rippleFrameDuration = 0.15f;

    public override void OnBoardSpawn(CellView cell, BoardManager board)
    {
        board.StartCoroutine(IdleRippleLoop(cell));
    }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        DrunkStateManager.Instance?.Sober();
    }

    private IEnumerator IdleRippleLoop(CellView cell)
    {
        if (rippleFrames == null || rippleFrames.Length == 0) yield break;

        int frame = 0;
        while (cell != null && cell.boardTile == this && !cell.Revealed)
        {
            cell.SetOverlaySprite(rippleFrames[frame]);
            frame = (frame + 1) % rippleFrames.Length;
            yield return new WaitForSeconds(rippleFrameDuration);
        }
    }
}