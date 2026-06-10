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
        if (interactedSpawnable == null)
        {
            if (cell.WasDirectlyClicked)
            {
                GameEvents.OnEmptyCellRevealed?.Invoke(cell); //EmptyCellRevealed event call
            }
            return;
        }

        if (cell.WasDirectlyClicked)
        {
            if (interactedSpawnable is MoleHoleSpawnableSO) return;

            if (interactedSpawnable is EnemySpawnableSO enemy)
            {
                GameEvents.OnEnemyRevealed?.Invoke(cell); //EnemyRevealed event call
                if (enemy.displayName == "Nathan") //Change to mine later
                {
                    GameData.Instance.FoundMine();
                    GameEvents.OnMineRevealed?.Invoke(cell); //MineFound event call
                }
                if (PlayerRunStats.Instance != null)
                    PlayerRunStats.Instance.ModifyHealth(-enemy.damage);

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
                        PlayerRunStats.Instance.ModifyHealth(-revealedEnemy.damage);

                    cell.spawnableBeforeAbilities = cell.spawnable;
                    if (revealedEnemy.abilities != null)
                    {
                        foreach (var ability in revealedEnemy.abilities)
                        {
                            if (ability != null) ability.OnReveal(cell, cell.boardManager);
                        }
                    }

                    if (cell.spawnable is MoleHoleSpawnableSO)
                        return;

                    TransitionToClearedState(cell);
                }
                break;

            case SpawnableType.Gold:
                if (cell.spawnable is GoldSpawnableSO goldData)
                {
                    GameEvents.OnGoldCellRevealed?.Invoke(cell); //GoldRevealed event call
                    GameData.Instance.CollectGold(goldData.goldValue);
                    TransitionToClearedState(cell);
                }
                break;

            case SpawnableType.Exit:
                GameEvents.OnExitRevealed?.Invoke(cell); //ExitRevealed event call
                GameData.Instance.StopGame();
                GameEvents.OnExitUsed?.Invoke(); //ExitUsed event call

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
        if (cell.State == CellState.Interacted)
        {
            GameData.Instance.DefeatedEnemy();
            GameEvents.OnEnemyDefeated?.Invoke(cell); //EnemyDefeated event call
            GameEvents.OnEnemyDefeatedCountReached?.Invoke(GameData.Instance.EnemiesDefeatedThisMatch); //EnemyDefeatedCountReached event call

            if (cell.spawnable is EnemySpawnableSO defeatedEnemy && defeatedEnemy.isBoss)
                GameEvents.OnBossDefeated?.Invoke(cell);

            if (cell.spawnable != null && cell.spawnable.abilities != null)
            {
                foreach (var ability in cell.spawnable.abilities)
                {
                    if (ability is PatternModifyDamageAbilitySO modAbility)
                        modAbility.RevertModifications(cell, cell.boardManager);

                    if (ability is ObscureAttackValuesAbilitySO obscureAbility)
                        obscureAbility.RevertObscure(cell, cell.boardManager);
                }
            }
        }

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