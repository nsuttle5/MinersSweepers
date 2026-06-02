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

        if (cell.WasDirectlyClicked)
        {
            if (cell.spawnable is EnemySpawnableSO enemy)
            {
                if (PlayerStats.Instance != null)
                {
                    Debug.Log($"Enemy hit. Took {-enemy.damage} damage");
                    PlayerStats.Instance.ModifyHealth(-enemy.damage);
                }
                TransitionToInteractedState(cell);
            }
            else
            {
                Debug.Log($"Hit smth that's not an enemy");

            }
        }
    }

    private void HandleRevealedClick(CellView cell)
    {
        if (cell.State == CellState.Interacted)
        {
            Debug.Log("Getting rid of dead enemy");
            TransitionToClearedState(cell);
            return;
        }

        if (cell.State != CellState.Revealed || cell.spawnable == null) return;

        switch (cell.spawnable.type)
        {
            case SpawnableType.Enemy:
                Debug.Log($"Attacking exposed enemy");
                TransitionToInteractedState(cell);
                break;
            case SpawnableType.Gold:
                if (cell.spawnable is GoldSpawnableSO goldData)
                {
                    Debug.Log("Grabbing Gold");
                    GameData.Instance.GoldFound += goldData.goldValue;
                    PlayerStats.Instance.ModifyGold(goldData.goldValue);
                    TransitionToClearedState(cell);
                }
                break;
            case SpawnableType.Exit:
                Debug.Log("Leaving room");
                if (SceneTransitionManager.Instance != null)
                    SceneTransitionManager.Instance.LoadScene("ResultsScreen");
                else
                    SceneManager.LoadScene("ResultsScreen");    
                break;
        }
    }

    private void TransitionToInteractedState(CellView cell)
    {
        cell.SetState(CellState.Interacted);
        if (cell.boardManager != null)
            cell.boardManager.RefreshAllCellDamageValues();
        else
            NotifyNeighborsOfThreatChange(cell);
    }

    private void TransitionToClearedState(CellView cell)
    {
        if (cell.State == CellState.Interacted) GameData.Instance.EnemiesDefeated++;
        cell.SetState(CellState.Cleared);
        if (cell.boardManager != null)
            cell.boardManager.RefreshAllCellDamageValues();
        else
            NotifyNeighborsOfThreatChange(cell);
    }

    private void NotifyNeighborsOfThreatChange(CellView cell)
    {
        int targetX = cell.x;
        int targetY = cell.y;
        BoardManager board = cell.boardManager;

        if (board == null) return;

        for (int nX = targetX - 1; nX <= targetX + 1; nX++)
            for (int nY = targetY - 1; nY <= targetY + 1; nY++)
            {
                if (nX == targetX && nY == targetY) continue;

                CellView adjacentCell = board.GetCellView(nX, nY);
                if (adjacentCell == null) continue;

                adjacentCell.TryDisplaySurroundingDamage();
                {
                    
                }
            }
    }
}
