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

    [Header("Safe Rows")]
    [SerializeField] private int safeRowCount = 3;

    private List<FishingRow> _activeRows = new();
    private Queue<FishingRow> _pool = new();

    private float _currentSpeed;
    private float _elapsedTime;
    private bool _isRunning = false;
    private int _totalRowsSpawned = 0;

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
        _totalRowsSpawned = 0;

        foreach (var row in _activeRows)
            ReturnToPool(row);
        _activeRows.Clear();

        float spawnY = bottomSpawnPoint.position.y;
        for (int i = 0; i < config.visibleRowBuffer; i++)
            SpawnRow(spawnY + i * config.cellSize);

        RefreshNeighborCounts();
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
                if (row.HasIncorrectlyMarkedCell())
                {
                    FishingGameManager.Instance.TriggerGameOver(GameOverReason.IncorrectMark);
                    return;
                }

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
        bool isSafe = _totalRowsSpawned < safeRowCount;
        FishingRow row = GetFromPool();
        row.transform.position = new Vector3(boardRoot.position.x, yPos, 0f);
        row.Setup(fishingCellPrefab, config.columnCount, config.cellSize, config, _elapsedTime, isSafe);
        row.hasPassedTop = false;
        _activeRows.Add(row);
        _totalRowsSpawned++;

        RefreshNeighborCounts();
    }

    private void HandleCellClicked(FishingCell cell)
    {
        if (!_isRunning) return;

        cell.Reveal();

        if (cell.cellType == FishingCellType.Bomb)
            FishingGameManager.Instance.TriggerGameOver(GameOverReason.BombClicked);
        else if (cell.cellType == FishingCellType.Fish)
            FishingGameManager.Instance.OnFishCaught();

        RefreshNeighborCounts();
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

    public FishingCell GetCell(int rowIndex, int colIndex)
    {
        if (rowIndex < 0 || rowIndex >= _activeRows.Count) return null;
        if (colIndex < 0 || colIndex >= _activeRows[rowIndex].cells.Count) return null;
        return _activeRows[rowIndex].cells[colIndex];
    }

    public void RefreshNeighborCounts()
    {
        for (int r = 0; r < _activeRows.Count; r++)
        {
            for (int c = 0; c < _activeRows[r].cells.Count; c++)
            {
                FishingCell cell = _activeRows[r].cells[c];
                if (!cell.isRevealed) continue;

                int bombCount = 0;

                for (int dr = -1; dr <= 1; dr++)
                {
                    for (int dc = -1; dc <= 1; dc++)
                    {
                        if (dr == 0 && dc == 0) continue;
                        FishingCell neighbor = GetCell(r + dr, c + dc);
                        if (neighbor != null && neighbor.cellType == FishingCellType.Bomb
                            && !neighbor.isRevealed && !neighbor.isDestroyed)
                            bombCount++;
                    }
                }

                cell.SetNeighborBombCount(bombCount);
            }
        }
    }
}

public enum GameOverReason { BombClicked, BombReachedTop, IncorrectMark }