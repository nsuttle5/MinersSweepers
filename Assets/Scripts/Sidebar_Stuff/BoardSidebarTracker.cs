using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class BoardSidebarTracker : MonoBehaviour
{
    public static BoardSidebarTracker Instance { get; private set; }

    public static UnityAction OnTrackerUpdated;

    private Dictionary<SpawnableSO, int> _spawnableCounts = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RegisterBoard(BoardManager board)
    {
        _spawnableCounts.Clear();

        for (int x = 0; x < board.Width; x++)
        {
            for (int y = 0; y < board.Height; y++)
            {
                CellView cell = board.GetCellView(x, y);
                if (cell == null || cell.spawnable == null) continue;
                AddSpawnable(cell.spawnable, silent: true);
            }
        }

        OnTrackerUpdated?.Invoke();
    }

    public void AddSpawnable(SpawnableSO spawnable, bool silent = false)
    {
        if (spawnable == null) return;
        if (!_spawnableCounts.ContainsKey(spawnable))
            _spawnableCounts[spawnable] = 0;
        _spawnableCounts[spawnable]++;

        if (!silent) OnTrackerUpdated?.Invoke();
    }

    public void RemoveSpawnable(SpawnableSO spawnable)
    {
        if (spawnable == null || !_spawnableCounts.ContainsKey(spawnable)) return;
        _spawnableCounts[spawnable]--;
        if (_spawnableCounts[spawnable] <= 0)
            _spawnableCounts.Remove(spawnable);

        OnTrackerUpdated?.Invoke();
    }

    public List<BoardSidebarEntry> GetEntries()
    {
        List<BoardSidebarEntry> entries = new();
        foreach (var kvp in _spawnableCounts)
        {
            if (kvp.Value <= 0) continue;
            bool discovered = LogbookManager.Instance != null && LogbookManager.Instance.IsDiscovered(kvp.Key);
            entries.Add(new BoardSidebarEntry(kvp.Key, kvp.Value, discovered));
        }
        entries.Sort((a, b) => b.spawnable.damage.CompareTo(a.spawnable.damage));
        return entries;
    }
}