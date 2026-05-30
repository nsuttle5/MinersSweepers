using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class BoardManager : MonoBehaviour
{
    public MinesweeperMapSO mapLayout;
    public GameObject cellPrefab;
    public Transform boardRoot;

    private int width, height;
    private GameObject[,] cellObjs;

    private bool firstClick = true;

    public static bool isLogbookOpen = false;

    public static UnityAction<CellView> OnCellRevealed;
    public static UnityAction<CellView> OnRevealedCellClick;

    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        if (!mapLayout || !cellPrefab || !boardRoot)
        {
            Debug.LogError("Assign references idiot");
            return;
        }

        firstClick = true;

        width = mapLayout.width;
        height = mapLayout.height;
        cellObjs = new GameObject[width, height];

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(boardRoot.GetChild(i).gameObject);

        List<Vector2Int> positions = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                positions.Add(new Vector2Int(x, y));

        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = positions[i]; positions[i] = positions[j]; positions[j] = temp;
        }

        List<SpawnableSO> toSpawn = new List<SpawnableSO>();
        foreach (var constraint in mapLayout.spawnConstraints)
        {
            int count = constraint.GetQuantity();
            for (int k = 0; k < count; k++)
                toSpawn.Add(constraint.spawnable);
        }

        SetGameData(toSpawn);

        for (int i = toSpawn.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            var temp = toSpawn[i]; toSpawn[i] = toSpawn[j]; toSpawn[j] = temp;
        }

        int t = 0;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            GameObject cell = Instantiate(cellPrefab, boardRoot);
            cell.transform.localPosition = new Vector3(pos.x, -pos.y, 0);
            cell.name = $"Cell_{pos.x}_{pos.y}";

            var cellComp = cell.GetComponent<CellView>();
            if (cellComp == null) cellComp = cell.AddComponent<CellView>();

            cellComp.boardManager = this;

            cellComp.x = pos.x;
            cellComp.y = pos.y;

            if (t < toSpawn.Count)
            {
                cellComp.spawnable = toSpawn[t];
                t++;
            }
            else
            {
                cellComp.spawnable = null;
            }
            cellComp.UpdateVisual();

            cellObjs[pos.x, pos.y] = cell;
        }
    }

    private void SetGameData(List<SpawnableSO> spawnables)
    {
        int mineCount = 0;
        int enemyCount = 0;
        int goldCount = 0;

        foreach (var spawnable in spawnables)
        {
            if (spawnable == null) continue;

            if (spawnable.displayName == "Nathan") mineCount++;
            else if (spawnable.type == SpawnableType.Enemy) enemyCount++;
            else if (spawnable.displayName == "Gold") goldCount++;
        }

        GameData.Instance.ResetData();

        GameData.Instance.TotalMines = mineCount;
        GameData.Instance.TotalEnemies = enemyCount;
        GameData.Instance.TotalGold = goldCount;
    }

    public void OnCellClicked(int cx, int cy)
    {
        if (isLogbookOpen) return;

        if (firstClick)
        {
            HandleFirstClick(cx, cy);
        }
        else
        {
            var cell = cellObjs[cx, cy].GetComponent<CellView>();
            if (cell == null) return;
            
            if (!cell.revealed)
            {
                cell.Reveal();
                OnCellRevealed?.Invoke(cell);
            }
            else
            {
                OnRevealedCellClick?.Invoke(cell);
            }
        }
    }

    private void HandleFirstClick(int cx, int cy)
    {
        var clickedCellView = cellObjs[cx, cy].GetComponent<CellView>();

        // Guarantee first-clicked cell is empty
        if (clickedCellView.spawnable != null)
        {
            var emptyCells = new List<CellView>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                {
                    var cell = cellObjs[x, y].GetComponent<CellView>();
                    if (cell.spawnable == null && !(cell.x == cx && cell.y == cy))
                    {
                        emptyCells.Add(cell);
                    }
                }
            if (emptyCells.Count > 0)
            {
                var swapWith = emptyCells[Random.Range(0, emptyCells.Count)];
                var temp = clickedCellView.spawnable;
                clickedCellView.spawnable = swapWith.spawnable;
                swapWith.spawnable = temp;
                clickedCellView.UpdateVisual();
                swapWith.UpdateVisual();
            }
        }

        // After the swap, reveal the full 3x3 radius as before
        for (int x = cx - 1; x <= cx + 1; x++)
        {
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var cell = cellObjs[x, y].GetComponent<CellView>();
                    if (cell != null && !cell.revealed)
                    {
                        cell.Reveal();
                        OnCellRevealed?.Invoke(cell);
                    }
                }
            }
        }

        GameData.Instance.GameStarted = true;

        firstClick = false;
    }

    public int GetNeighborDamage(int cx, int cy)
    {
        int total = 0;
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                if (x == cx && y == cy) continue; // Skip self
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var neighbor = cellObjs[x, y].GetComponent<CellView>();
                    if (neighbor != null && neighbor.spawnable != null
                        && neighbor.spawnable.type == SpawnableType.Enemy)
                    {
                        total += neighbor.spawnable.damage;
                    }
                }
            }
        return total;
    }
}