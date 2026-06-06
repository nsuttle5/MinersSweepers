using UnityEngine;
using System.Collections.Generic;

public class FishingBoardManager : MonoBehaviour
{
    public static FishingBoardManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private FishingConfigSO config;

    [Header("References")]
    [SerializeField] private GameObject fishingRowPrefab;
    [SerializeField] private GameObject fishingCellPrefab;
    [SerializeField] private Transform boardRoot;
    [SerializeField] private Transform topBoundary;
    [SerializeField] private Transform bottomSpawnPoint;

    private List<FishingRow> _activeRows = new();
    private Queue<FishingRow> _pool = new();

    private float _currentSpeed;
    private float _elapsedTime;
    private bool _isRunning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void OnEnable()
    {
        FishingCell.OnCellClicked += HandleCellClicked;
    }

    private void OnDisable()
    {
        FishingCell.OnCellClicked -= HandleCellClicked;
    }

    public void StartGame()
    {
        _currentSpeed = config.initialScrollSpeed;
        _elapsedTime = 0f;
        _isRunning = true;

        foreach (var row in _activeRows)
            ReturnToPool(row);
        _activeRows.Clear();

        float spawnY = bottomSpawnPoint.position.y;
        for (int i = 0; i < config.visibleRowBuffer; i++)
            SpawnRow(spawnY + i * config.cellSize);
    }

    public void StopGame()
    {
        _isRunning = false;
    }

    private void Update()
    {
        if (!_isRunning) return;

        _elapsedTime += Time.deltaTime;
        _currentSpeed = Mathf.Min(
            config.initialScrollSpeed + config.speedIncreaseRate * _elapsedTime,
            config.maxScrollSpeed);

        for (int i = _activeRows.Count - 1; i >= 0; i--)
        {
            FishingRow row = _activeRows[i];
            row.transform.position += Vector3.up * _currentSpeed * Time.deltaTime;

            if (row.transform.position.y >= topBoundary.position.y)
            {
                if (row.HasUnrevealedBomb())
                {
                    FishingGameManager.Instance.TriggerGameOver(GameOverReason.BombReachedTop);
                    return;
                }

                row.MarkAllDestroyed();
                ReturnToPool(row);
                _activeRows.RemoveAt(i);

                SpawnRow(bottomSpawnPoint.position.y);
            }
        }
    }

    private void SpawnRow(float yPos)
    {
        FishingRow row = GetFromPool();
        row.transform.position = new Vector3(boardRoot.position.x, yPos, 0f);
        row.Setup(fishingCellPrefab, config.columnCount, config.cellSize, config, _elapsedTime);
        row.hasPassedTop = false;
        _activeRows.Add(row);
    }

    private void HandleCellClicked(FishingCell cell)
    {
        if (!_isRunning) return;

        cell.Reveal();

        if (cell.cellType == FishingCellType.Bomb)
            FishingGameManager.Instance.TriggerGameOver(GameOverReason.BombClicked);
        else if (cell.cellType == FishingCellType.Fish)
            FishingGameManager.Instance.OnFishCaught();
    }

    private FishingRow GetFromPool()
    {
        if (_pool.Count > 0)
        {
            var row = _pool.Dequeue();
            row.gameObject.SetActive(true);
            return row;
        }
        return Instantiate(fishingRowPrefab, boardRoot).GetComponent<FishingRow>();
    }

    private void ReturnToPool(FishingRow row)
    {
        row.gameObject.SetActive(false);
        _pool.Enqueue(row);
    }

    public float GetElapsedTime() => _elapsedTime;
}

public enum GameOverReason { BombClicked, BombReachedTop }