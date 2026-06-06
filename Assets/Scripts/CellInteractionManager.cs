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
        SpawnableSO interactedSpawnable = (cell != null) ? cell.spawnable : null;
        if (interactedSpawnable == null) return;

        if (cell.WasDirectlyClicked)
        {
            if (interactedSpawnable is MoleHoleSpawnableSO) return;

            if (interactedSpawnable is EnemySpawnableSO enemy)
            {
                if (PlayerRunStats.Instance != null)
                {
                    PlayerRunStats.Instance.ModifyHealth(-enemy.damage);
                }
                TransitionToInteractedState(cell);
            }
        }
    }

    private void HandleRevealedClick(CellView cell)
    {
        if (cell.State == CellState.Interacted)
        {
            TransitionToClearedState(cell);
            return;
        }

        if (cell.State != CellState.Revealed || cell.spawnable == null) return;

        switch (cell.spawnable.type)
        {
            case SpawnableType.Enemy:
                if (cell.spawnable is MoleHoleSpawnableSO)
                {
                    TransitionToInteractedState(cell);
                    break;
                }

                if (cell.spawnable is EnemySpawnableSO revealedEnemy)
                {
                    if (PlayerRunStats.Instance != null)
                    {
                        PlayerRunStats.Instance.ModifyHealth(-revealedEnemy.damage);
                    }

                    cell.spawnableBeforeAbilities = cell.spawnable;
                    if (revealedEnemy.abilities != null)
                    {
                        foreach (var ability in revealedEnemy.abilities)
                        {
                            if (ability != null) ability.OnReveal(cell, cell.boardManager);
                        }
                    }
                }
                TransitionToInteractedState(cell);
                break;
            case SpawnableType.Gold:
                if (cell.spawnable is GoldSpawnableSO goldData)
                {
                    GameData.Instance.CollectGold(goldData.goldValue);
                    TransitionToClearedState(cell);
                }
                break;
            case SpawnableType.Exit:
                GameData.Instance.StopGame();

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
        if (cell.State == CellState.Interacted) GameData.Instance.DefeatedEnemy();
        cell.spawnable = null;

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
            }
    }
}
