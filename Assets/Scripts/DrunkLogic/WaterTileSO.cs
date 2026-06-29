using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "WaterTile", menuName = "BoardTiles/WaterTile")]
public class WaterTileSO : BoardTileSO
{
    [Header("Splash Animation")]
    public Sprite[] splashFrames;
    public float splashFrameDuration = 0.06f;

    [Header("Ripple")]
    public Color rippleColor = new Color(0.4f, 0.8f, 1f, 1f);
    public float rippleSpeed = 3f;

    public override void OnBoardSpawn(CellView cell, BoardManager board)
    {
        board.StartCoroutine(RippleWhileHidden(cell));
    }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        board.StartCoroutine(SplashThenSober(cell, board));
    }

    private IEnumerator RippleWhileHidden(CellView cell)
    {
        float t = 0f;
        while (cell != null && cell.boardTile == this && !cell.Revealed)
        {
            t += Time.deltaTime * rippleSpeed;
            float blend = (Mathf.Sin(t) + 1f) * 0.5f;
            cell.SetTintOverride(Color.Lerp(Color.white, rippleColor, blend));
            yield return null;
        }
        if (cell != null) cell.SetTintOverride(Color.white);
    }

    private IEnumerator SplashThenSober(CellView cell, BoardManager board)
    {
        if (splashFrames != null && splashFrames.Length > 0)
        {
            GameObject splashObj = new GameObject("WaterSplash");
            splashObj.transform.position = cell.transform.position;
            splashObj.transform.SetParent(board.boardRoot);

            SpriteRenderer sr = splashObj.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 100;

            foreach (var frame in splashFrames)
            {
                sr.sprite = frame;
                yield return new WaitForSeconds(splashFrameDuration);
            }

            Object.Destroy(splashObj);
        }

        DrunkStateManager.Instance?.Sober();
    }
}