using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class BoardManager : MonoBehaviour
{
    public MinesweeperMapSO mapLayout;
    public GameObject cellPrefab;
    public Transform boardRoot;

    [SerializeField] private float verticalSpacing = 1f;

    private int width, height;
    private GameObject[,] cellObjs;

    public List<CellView> unrevealedCells = new();
    public List<CellView> revealedCells = new();

    private bool firstClick = true;
    public static bool isLogbookOpen = false;

    public static UnityAction<CellView> OnCellRevealed;
    public static UnityAction<CellView> OnRevealedCellClick;

    public int Width => width;
    public int Height => height;
    public CellView GetCellView(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        if (cellObjs[x,y] == null) return null;
        return cellObjs[x, y].GetComponent<CellView>();
    }

    private void Start()
    {
        if (MapManager.Instance == null) return;
        if (MapManager.Instance.currentNode.type is not MapLevelNodeTypeSO) return;

        mapLayout = (MapManager.Instance.currentNode.type as MapLevelNodeTypeSO).mapLayout;
        GenerateBoard();
    }

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
        unrevealedCells.Clear();
        revealedCells.Clear();

        for (int i = boardRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(boardRoot.GetChild(i).gameObject);

        List<Vector2Int> positions = new();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                positions.Add(new Vector2Int(x, y));

        for (int i = positions.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (positions[j], positions[i]) = (positions[i], positions[j]);
        }

        List<SpawnableSO> toSpawn = new();
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
            (toSpawn[j], toSpawn[i]) = (toSpawn[i], toSpawn[j]);
        }

        int t = 0;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            GameObject cell = Instantiate(cellPrefab, boardRoot);
            cell.transform.localPosition = new Vector3(pos.x, -pos.y * verticalSpacing, 0);
            cell.name = $"Cell_{pos.x}_{pos.y}";

            if (!cell.TryGetComponent(out CellView cellComp)) cellComp = cell.AddComponent<CellView>();

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

            // Add all to unrevealed at the start
            unrevealedCells.Add(cellComp);
        }
    }

    private void SetGameData(List<SpawnableSO> spawnables)
    {
        int mineCount = 0, enemyCount = 0, goldCount = 0;
        foreach (var spawnable in spawnables)
        {
            if (spawnable == null) continue;
            if (spawnable.displayName == "Nathan") mineCount++;
            else if (spawnable.type == SpawnableType.Enemy) enemyCount++;
            else if (spawnable.type == SpawnableType.Gold) goldCount++;
        }

        GameData.Instance.ResetMineData();
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
            if (!cellObjs[cx, cy].TryGetComponent(out CellView cell)) return;
            if (!cell.Revealed)
            {
                cell.Reveal(wasDirectClick: true);
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
        if (clickedCellView.spawnable != null)
        {
            var emptyCells = new List<CellView>();
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (cellObjs[x, y].GetComponent<CellView>().spawnable == null && !(x == cx && y == cy))
                        emptyCells.Add(cellObjs[x, y].GetComponent<CellView>());
            if (emptyCells.Count > 0)
            {
                CellView swapWith = emptyCells[Random.Range(0, emptyCells.Count)];
                (swapWith.spawnable, clickedCellView.spawnable) = (clickedCellView.spawnable, swapWith.spawnable);
                clickedCellView.UpdateVisual();
                swapWith.UpdateVisual();
            }
        }

        clickedCellView.Reveal(wasDirectClick: true);
        OnCellRevealed?.Invoke(clickedCellView);

        for (int x = cx - 1; x <= cx + 1; x++)
        {
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                if (x == cx && y == cy) continue;

                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var cell = cellObjs[x, y].GetComponent<CellView>();
                    if (cell != null && !cell.Revealed)
                    {
                        cell.Reveal(wasDirectClick: false, triggerAbilities: true);
                        OnCellRevealed?.Invoke(cell);
                    }
                }
            }
        }

        GameData.Instance.GameStarted = true;
        firstClick = false;
        RefreshAllCellDamageValues();
    }

    public void NotifyCellRevealed(CellView cell)
    {
        if (unrevealedCells.Contains(cell))
            unrevealedCells.Remove(cell);
        if (!revealedCells.Contains(cell))
            revealedCells.Add(cell);
    }

    public void RefreshAllCellDamageValues()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                CellView cell = GetCellView(x, y);
                if (cell == null) continue;

                cell.TryDisplaySurroundingDamage();
            }
    }

    public int GetNeighborDamage(int cx, int cy)
    {
        int total = 0;
        for (int x = cx - 1; x <= cx + 1; x++)
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                if (x == cx && y == cy) continue;
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    var neighbor = cellObjs[x, y].GetComponent<CellView>();
                    if (neighbor != null && neighbor.IsActiveThreat)
                        total += neighbor.spawnable.damage;
                }
            }
        return total;
    }
}