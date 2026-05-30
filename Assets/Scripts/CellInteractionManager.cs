using UnityEngine;
using UnityEngine.Events;

public class CellInteractionManager : MonoBehaviour
{
    public static UnityAction OnExitTileClicked;

    private void OnEnable()
    {
        BoardManager.OnCellRevealed += HandleCellReveal;
        BoardManager.OnRevealedCellClick += HandleRevealedClick;
    }

    private void OnDisable()
    {
        BoardManager.OnCellRevealed -= HandleCellReveal;
        BoardManager.OnRevealedCellClick -= HandleRevealedClick;
    }

    private void HandleCellReveal(CellView cell)
    {
        if (cell.spawnable == null) return;

        switch (cell.spawnable.type)
        {
            case SpawnableType.Enemy:
                break;
            case SpawnableType.Exit:
                break;
            default:
                break;
        }
    }

    private void HandleRevealedClick(CellView cell)
    {
        if (cell.spawnable == null) return;

        switch (cell.spawnable.type)
        {
            case SpawnableType.Enemy:
                break;
            case SpawnableType.Exit:
                Debug.Log("Exit Tile Clicked!");
                OnExitTileClicked?.Invoke();
                break;
            default:
                break;
        }
    }
}
