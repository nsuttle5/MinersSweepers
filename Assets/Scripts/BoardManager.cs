using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Collections;

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

    private int totalCells;

    [Header("Spawn Animation")]
    [SerializeField] private float popScaleUp = 1.2f;
    [SerializeField] private float popDuration = 0.15f;
    [SerializeField] private float popHoldDuration = .05f;
    [SerializeField] private float popSettleDuration = 0.1f;
    [SerializeField] private float diagonalDelay = 0.04f;

    private Coroutine _spawnAnimationCoroutine;

    [Header("Board Tiles")]
    [SerializeField] private List<BoardTileSpawnRule> activeTileSpawnRules = new();

    public static BoardManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public CellView GetCellView(int x, int y)
    {
        if (x < 0 || y < 0 || x >= width || y >= height) return null;
        if (cellObjs[x, y] == null) return null;
        return cellObjs[x, y].GetComponent<CellView>();
    }

    private void Start()
    {
        if (MapManager.Instance == null) return;
        if (MapManager.Instance.currentNode.type is not MapLevelNodeTypeSO) return;

        if (CharacterManager.Instance != null && CharacterManager.Instance.HasSelection)
            ArtifactManager.Instance.ApplyCharacterLoadout(CharacterManager.Instance.SelectedCharacter);

        mapLayout = (MapManager.Instance.currentNode.type as MapLevelNodeTypeSO).mapLayout;
        GenerateBoard();
    }

    private IEnumerator AnimateBoardSpawn()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellView cell = GetCellView(x, y);
                if (cell != null)
                {
                    cell.transform.localScale = Vector3.zero;
                }
            }
        }

        int maxDiagonal = (width - 1) + (height - 1);

        for (int diagonal = 0; diagonal <= maxDiagonal; diagonal++)
        {
            for (int x = 0; x < width; x++)
            {
                int y = diagonal - x;
                if (y < 0 || y >= height) continue;

                CellView cell = GetCellView(x, y);
                if (cell != null)
                    StartCoroutine(PopCell(cell));
            }

            yield return new WaitForSeconds(diagonalDelay);
        }
    }
    private IEnumerator PopCell(CellView cell)
    {
        if (cell == null) yield break;
        Vector3 targetScale = Vector3.one;
        Vector3 overshootScale = Vector3.one * popScaleUp;

        yield return ScaleCell(cell.transform, Vector3.zero, overshootScale, popDuration);
        if (cell == null) yield break;

        yield return new WaitForSeconds(popHoldDuration);
        if (cell == null) yield break;

        yield return ScaleCell(cell.transform, overshootScale, targetScale, popSettleDuration);
    }

    private IEnumerator ScaleCell(Transform t, Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            float e = Mathf.Clamp01(elapsed / duration);
            t.localScale = Vector3.Lerp(from, to, e);
            yield return null;
        }
        if (t != null) t.localScale = to;
    }

    private void PlaceBoardTiles()
    {
        foreach (var rule in activeTileSpawnRules)
        {
            if (rule.tileType == null) continue;
            if (Random.value > rule.spawnChance) continue;

            List<CellView> candidates = new();
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    CellView cell = GetCellView(x, y);
                    if (cell == null || cell.boardTile != null) continue;

                    bool hasEnemy = cell.spawnable is EnemySpawnableSO;
                    bool isEmpty = cell.spawnable == null;

                    if (rule.requiresEnemyUnderneath && !hasEnemy) continue;
                    if (rule.requiresEmptyUnderneath && !isEmpty) continue;

                    candidates.Add(cell);
                }
            }

            if (candidates.Count == 0) continue;

            CellView chosen = candidates[Random.Range(0, candidates.Count)];
            chosen.boardTile = rule.tileType;
            chosen.UpdateVisual();
            rule.tileType.OnBoardSpawn(chosen, this);
        }
    }

    [ContextMenu("Generate Board")]
    public void GenerateBoard()
    {
        if (_spawnAnimationCoroutine != null)
        {
            StopCoroutine(_spawnAnimationCoroutine);
            _spawnAnimationCoroutine = null;
        }
        StopAllCoroutines();

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

        List<SpawnConstraint> sortedConstraints = new(mapLayout.spawnConstraints);
        sortedConstraints.Sort((a, b) => ((int)a.priority).CompareTo((int)b.priority));

        List<SpawnableSO> toSpawn = ShuffleWithinPriorityGroups(sortedConstraints);

        SetGameData(toSpawn);

        int t = 0;
        for (int i = 0; i < positions.Count; i++)
        {
            Vector2Int pos = positions[i];
            GameObject cell = Instantiate(cellPrefab, boardRoot);
            cell.transform.localPosition = new Vector3(pos.x, -pos.y * verticalSpacing, 0);
            cell.name = $"Cell_{pos.x}_{pos.y}";

            if (!cell.TryGetComponent(out CellView cellComp))
                cellComp = cell.AddComponent<CellView>();

            cellComp.boardManager = this;
            cellComp.x = pos.x;
            cellComp.y = pos.y;
            cellComp.spawnable = t < toSpawn.Count ? toSpawn[t++] : null;

            cellObjs[pos.x, pos.y] = cell;
            unrevealedCells.Add(cellComp);
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellView cell = GetCellView(x, y);
                if (cell == null || cell.spawnable == null || cell.spawnable.abilities == null) continue;

                foreach (var ability in cell.spawnable.abilities)
                {
                    if (ability is ISpawnableOnBoard boardAbility)
                        boardAbility.OnBoardSpawn(cell, this);
                }
            }
        }

        PlaceMapConfigBoardTiles();
        PlaceBoardTiles();
        RefreshAllCellVisuals();
        _spawnAnimationCoroutine = StartCoroutine(AnimateBoardSpawn());
        totalCells = CountNonVoidCells();
        GameEvents.OnBoardGenerated?.Invoke(); //Board Generated event call

        if (BoardSidebarTracker.Instance != null)
        {
            BoardSidebarTracker.Instance.RegisterBoard(this);
        }

        StartCoroutine(AnimateBoardSpawn());
    }

    private List<SpawnableSO> ShuffleWithinPriorityGroups(List<SpawnConstraint> sortedConstraints)
    {
        List<SpawnableSO> result = new();

        int i = 0;
        while (i < sortedConstraints.Count)
        {
            SpawnPriority currentPriority = sortedConstraints[i].priority;
            List<SpawnableSO> group = new();

            while (i < sortedConstraints.Count && sortedConstraints[i].priority == currentPriority)
            {
                int count = sortedConstraints[i].GetQuantity();
                for (int k = 0; k < count; k++)
                    group.Add(sortedConstraints[i].spawnable);
                i++;
            }

            for (int j = group.Count - 1; j > 0; j--)
            {
                int r = Random.Range(0, j + 1);
                (group[r], group[j]) = (group[j], group[r]);
            }

            result.AddRange(group);
        }

        return result;
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

        GameData.Instance.SetMineData(goldCount, mineCount, enemyCount);
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
                {
                    var candidate = cellObjs[x, y].GetComponent<CellView>();
                    if (candidate.spawnable == null && !(x == cx && y == cy))
                        emptyCells.Add(candidate);
                }
            if (emptyCells.Count > 0)
            {
                CellView swapWith = emptyCells[Random.Range(0, emptyCells.Count)];
                (swapWith.spawnable, clickedCellView.spawnable) = (clickedCellView.spawnable, swapWith.spawnable);
                clickedCellView.UpdateVisual();
                swapWith.UpdateVisual();
            }
        }

        firstClick = false;
        GameData.Instance.StartGame();
        GameEvents.OnFirstCellRevealed?.Invoke(); //First Cell Revealed event call


        clickedCellView.Reveal(wasDirectClick: true, triggerAbilities: false);
        OnCellRevealed?.Invoke(clickedCellView);

        for (int x = cx - 1; x <= cx + 1; x++)
        {
            for (int y = cy - 1; y <= cy + 1; y++)
            {
                if (x == cx && y == cy) continue;
                if (x < 0 || x >= width || y < 0 || y >= height) continue;

                var cell = cellObjs[x, y].GetComponent<CellView>();
                if (cell != null && !cell.Revealed)
                {
                    if (cell.spawnable == null)
                    {
                        cell.Reveal(wasDirectClick: false, triggerAbilities: false);
                        OnCellRevealed?.Invoke(cell);
                    }
                    else
                    {
                        cell.isKnown = true;
                        cell.UpdateVisual();
                    }
                }
            }
        }

        RefreshAllCellDamageValues();
        RefreshAllCellVisuals();
    }

    public void NotifyCellRevealed(CellView cell)
    {
        if (unrevealedCells.Contains(cell))
            unrevealedCells.Remove(cell);
        if (!revealedCells.Contains(cell))
            revealedCells.Add(cell);

        cell.SetPartialReveal(false);

        CellView cellAbove = GetCellView(cell.x, cell.y - 1);
        if (cellAbove != null && !cellAbove.Revealed)
            cellAbove.SetPartialReveal(true);

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;
                CellView neighbor = GetCellView(cell.x + dx, cell.y + dy);
                if (neighbor != null)
                    neighbor.UpdateVisual();
            }
        }

        GameEvents.OnCellRevealed?.Invoke(cell); //CellRevealed event call

        float percent = (float)revealedCells.Count / totalCells;
        GameEvents.OnBoardPercentRevealed?.Invoke(percent); //BoardPercentRevealed event call

        int revealed = revealedCells.Count;
        //Run at arbitrary number
        GameEvents.OnCellRevealedCountReached?.Invoke(revealed); //CellRevealedCountReached event call
    }

    public bool IsSurroundedByRevealed(int cellX, int cellY)
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int checkX = cellX + dx;
                int checkY = cellY + dy;

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    CellView neighbor = GetCellView(checkX, checkY);
                    if (neighbor == null || !neighbor.Revealed) return false;
                }
            }
        }
        return true;
    }

    public void RefreshAllCellVisuals()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                CellView cell = GetCellView(x, y);
                if (cell != null)
                    cell.UpdateVisual();
            }
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
                        total += neighbor.EffectiveDamage;
                }
            }
        return total;
    }

    public void OnCellRevealedNotify(CellView cell)
    {
        OnCellRevealed?.Invoke(cell);
    }

    public void NotifyCellHidden(CellView cell)
    {
        if (revealedCells.Contains(cell))
            revealedCells.Remove(cell);
        if (!unrevealedCells.Contains(cell))
            unrevealedCells.Add(cell);

        GameEvents.OnCellHidden?.Invoke(cell); //CellHidden event call
    }

    public bool TryPlaceBoardTile(BoardTileSO tileType, bool requiresEnemy, bool requiresEmpty)
    {
        List<CellView> candidates = new();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellView cell = GetCellView(x, y);
                if (cell == null || cell.boardTile != null) continue;

                bool hasEnemy = cell.spawnable is EnemySpawnableSO;
                bool isEmpty = cell.spawnable == null;

                if (requiresEnemy && !hasEnemy) continue;
                if (requiresEmpty && !isEmpty) continue;

                candidates.Add(cell);
            }
        }

        if (candidates.Count == 0) return false;

        CellView chosen = candidates[Random.Range(0, candidates.Count)];
        chosen.boardTile = tileType;
        chosen.UpdateVisual();
        tileType.OnBoardSpawn(chosen, this);

        return true;
    }

    private void PlaceMapConfigBoardTiles()
    {
        if (mapLayout.boardTileConstraints == null) return;

        foreach (var constraint in mapLayout.boardTileConstraints)
        {
            if (constraint.tileType == null) continue;

            int quantity = constraint.GetQuantity();
            for (int i = 0; i < quantity; i++)
            {
                if (Random.value > constraint.spawnChance) continue;

                TryPlaceBoardTile(
                    constraint.tileType,
                    constraint.requiresEnemyUnderneath,
                    constraint.requiresEmptyUnderneath);
            }
        }
    }

    private int CountNonVoidCells()
    {
        int count = 0;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                CellView cell = GetCellView(x, y);
                if (cell != null && !cell.isVoid)
                {
                    count++;
                }
            }
        }
        return count;
    }
}