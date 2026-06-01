using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class CellInteractionManager : MonoBehaviour
{
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
                if (cell.spawnable.displayName == "Nathan") GameData.Instance.MinesFound++;
                else GameData.Instance.EnemiesDefeated++;
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
            case SpawnableType.Gold:
                GameData.Instance.GoldFound++;
                cell.spawnable = null;
                cell.UpdateVisual();
                cell.TryDisplaySurroundingDamage();
                break;
            case SpawnableType.Exit:
                Debug.Log("Exit Tile Clicked!");
                GameData.Instance.GameStarted = false;

                if (SceneTransitionManager.Instance) SceneTransitionManager.Instance.LoadScene("ResultsScreen");
                else SceneManager.LoadScene("ResultsScreen");
                break;
            default:
                break;
        }
    }
}
