using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "AncientWineChalice",
    menuName = "Items/Artifacts/AncientWineChalice")]
public class AncientWineChaliceArtifact : ArtifactSO, ISpawnableOnBoard
{
    [Header("Health Settings")]
    public int maxHPBoost = 50;
    public float hpDrainPerSecond = 1f;
    public int drainAmount = 1;
    private int _baseMaxHpBeforeBoost = -1;

    [Header("Beer Spawning")]
    public float beerSpawnIntervalSeconds = 15f;
    public SpawnableSO beerMugSpawnable;

    [Header("Water Tile")]
    public BoardTileSO waterTile;

    private Coroutine _drainCoroutine;
    private Coroutine _beerCoroutine;
    private bool _activeInMine = false;

    protected override void Subscribe()
    {
        GameEvents.OnBoardGenerated += OnBoardGenerated;
        GameEvents.OnRunEnd += OnRunEnd;
        DrunkStateManager.OnSobered += HandleSobered;
    }

    protected override void Unsubscribe()
    {
        GameEvents.OnBoardGenerated -= OnBoardGenerated;
        GameEvents.OnRunEnd -= OnRunEnd;
        DrunkStateManager.OnSobered -= HandleSobered;
    }

    private void OnBoardGenerated()
    {
        if (PlayerRunStats.Instance == null) return;

        if (_baseMaxHpBeforeBoost < 0)
            _baseMaxHpBeforeBoost = PlayerRunStats.Instance.MaxHp;

        PlayerRunStats.Instance.SetTempMaxHP(_baseMaxHpBeforeBoost + maxHPBoost);
        PlayerRunStats.Instance.ModifyHealth(maxHPBoost);

        DrunkStateManager.Instance?.StartDrunk();
        _activeInMine = true;

        if (waterTile != null && BoardManager.Instance != null)
            BoardManager.Instance.TryPlaceBoardTile(waterTile, false, true);

        StopMineCoroutines();
        _drainCoroutine = ArtifactCoroutineRunner.Instance.StartTracked(DrainLoop());
        _beerCoroutine = ArtifactCoroutineRunner.Instance.StartTracked(BeerSpawnLoop());
    }

    private void OnRunEnd()
    {
        StopMineCoroutines();
        _activeInMine = false;

        if (PlayerRunStats.HasInstance && _baseMaxHpBeforeBoost >= 0)
        {
            PlayerRunStats.Instance.SetTempMaxHP(_baseMaxHpBeforeBoost);
            _baseMaxHpBeforeBoost = -1;
        }

        DrunkStateManager.Instance?.Sober();
    }

    private void HandleSobered()
    {
        if (!PlayerRunStats.HasInstance || _baseMaxHpBeforeBoost < 0) return;

        if (PlayerRunStats.Instance.CurrentHP > _baseMaxHpBeforeBoost)
        {
            PlayerRunStats.Instance.CurrentHP = _baseMaxHpBeforeBoost;
            PlayerRunStats.Instance.OnHealthChanged?.Invoke(
                PlayerRunStats.Instance.CurrentHP, PlayerRunStats.Instance.MaxHp);
            GameEvents.OnHealthChanged?.Invoke(
                PlayerRunStats.Instance.CurrentHP, PlayerRunStats.Instance.MaxHp);
        }

        StopMineCoroutines();
    }

    private IEnumerator DrainLoop()
    {
        while (_activeInMine && DrunkStateManager.Instance != null
            && DrunkStateManager.Instance.IsDrunk)
        {
            yield return new WaitForSeconds(1f / hpDrainPerSecond);

            if (!PlayerRunStats.HasInstance) yield break;
            if (!DrunkStateManager.Instance.IsDrunk) yield break;

            PlayerRunStats.Instance.ModifyHealth(-drainAmount);
        }
    }

    private IEnumerator BeerSpawnLoop()
    {
        while (_activeInMine && DrunkStateManager.Instance != null
            && DrunkStateManager.Instance.IsDrunk)
        {
            yield return new WaitForSeconds(beerSpawnIntervalSeconds);

            if (!_activeInMine) yield break;
            if (BoardManager.Instance == null) yield break;
            if (beerMugSpawnable == null) yield break;

            BoardManager.Instance.SpawnOnRandomEmptyCell(beerMugSpawnable);
        }
    }

    private void StopMineCoroutines()
    {
        if (_drainCoroutine != null)
        {
            ArtifactCoroutineRunner.Instance?.StopTracked(_drainCoroutine);
            _drainCoroutine = null;
        }
        if (_beerCoroutine != null)
        {
            ArtifactCoroutineRunner.Instance?.StopTracked(_beerCoroutine);
            _beerCoroutine = null;
        }
    }

    public void OnBoardSpawn(CellView sourceCell, BoardManager board) { }
}