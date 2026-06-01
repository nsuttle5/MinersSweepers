using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    [Header("Map Generation")]
    [SerializeField] MapGenConfigSO mapConfig;
    [SerializeField] List<MapNodeTypeSO> allNodeTypes = new();
    [SerializeField] Transform mapRoot;
    [SerializeField] GameObject nodePrefab;
    [SerializeField] bool mapAlreadyGenerated = false;

    [Header("Node Anchors")]
    [SerializeField] Transform startAnchor;
    [SerializeField] Transform endAnchor;

    [Header("Node Bounding Box")]
    [SerializeField] Transform boundingBoxMin;
    [SerializeField] Transform boundingBoxMax;
    [SerializeField] bool useBoundingBox = true;

    [Header("Connection Lines")]
    [SerializeField] GameObject linePrefab;
    [SerializeField, Range(0f, 80f)] float lineCutDistance = 40f;
    [SerializeField, Range(1f, 20f)] float lineThickness = 5f;
    [SerializeField] Color lineColor = Color.white;

    [Header("Node Interactable Colors")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color disabledColor = Color.gray;

    List<List<MapNode>> nodeRows = new();
    public MapNode currentNode;
    public static MapManager Instance { get; private set; }

    Vector2 ClampPositionToBoundingBox(Vector2 position)
    {
        if (!useBoundingBox || boundingBoxMin == null || boundingBoxMax == null)
            return position;

        Vector2 minPos = mapRoot.GetComponent<RectTransform>().InverseTransformPoint(boundingBoxMin.position);
        Vector2 maxPos = mapRoot.GetComponent<RectTransform>().InverseTransformPoint(boundingBoxMax.position);

        return new Vector2(
            Mathf.Clamp(position.x, Mathf.Min(minPos.x, maxPos.x), Mathf.Max(minPos.x, maxPos.x)),
            Mathf.Clamp(position.y, Mathf.Min(minPos.y, maxPos.y), Mathf.Max(minPos.y, maxPos.y))
        );
    }

    [ContextMenu("GenerateMap")]
    public void GenerateMap()
    {
        if (startAnchor == null || endAnchor == null)
        {
            Debug.LogError("Start and End anchors must be assigned!");
            return;
        }

        mapAlreadyGenerated = false;
        currentNode = null;
        nodeRows.Clear();

#if UNITY_EDITOR
        foreach (Transform child in mapRoot)
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
#else
        foreach (Transform child in mapRoot) Destroy(child.gameObject);
#endif

        int levels = mapConfig.numLevels;
        Vector2 startPos = mapRoot.GetComponent<RectTransform>().InverseTransformPoint(startAnchor.position);
        Vector2 endPos = mapRoot.GetComponent<RectTransform>().InverseTransformPoint(endAnchor.position);

        for (int level = 0; level < levels; level++)
        {
            int nodesInRow = mapConfig.nodesPerLevel[level].GetValue();
            var weightsData = mapConfig.weightsPerLevel.Find(w => w.level == level);
            var weights = (weightsData != null && weightsData.nodeWeights.Count > 0) ? weightsData.nodeWeights : mapConfig.weightsPerLevel[0].nodeWeights;
            var rowList = new List<MapNode>();

            float levelProgress = levels > 1 ? (float)level / (levels - 1) : 0f;
            float xPos = Mathf.Lerp(startPos.x, endPos.x, levelProgress);

            for (int n = 0; n < nodesInRow; n++)
            {
                MapNode node = new()
                {
                    levelIndex = level,
                    nodeIndex = n,
                    type = PickWeightedNodeType(weights)
                };
                rowList.Add(node);

                var go = Instantiate(nodePrefab, mapRoot);
                var button = go.GetComponent<MapNodeButton>();
                node.buttonUI = button;
                if (button != null)
                {
                    button.Set(node, normalColor, disabledColor);
                    button.button.onClick.RemoveAllListeners();
                    button.button.onClick.AddListener(() => button.OnClick());
                }

                var rt = go.GetComponent<RectTransform>();
                float totalColHeight = (nodesInRow - 1) * (endPos.y - startPos.y) / (nodesInRow > 1 ? nodesInRow - 1 : 1);
                float yOffset = n * totalColHeight / (nodesInRow > 1 ? nodesInRow - 1 : 1) - totalColHeight / 2f;
                float yPos = Mathf.Lerp(startPos.y, endPos.y, levelProgress) + yOffset;

                Vector2 finalPos = new Vector2(xPos, yPos);
                finalPos = ClampPositionToBoundingBox(finalPos);
                rt.anchoredPosition = finalPos;
            }
            nodeRows.Add(rowList);
        }

        GenerateConnections();
        DrawAllConnections();
        UpdateNodeInteractability();
        mapAlreadyGenerated = true;
    }

    [ContextMenu("ForceGenerateMap")]
    public void ForceGenerateMap()
    {
        mapAlreadyGenerated = false;
        currentNode = null;
        nodeRows.Clear();
        GenerateMap();
    }

    public void RebuildMapUI(Transform newMapRoot)
    {
#if UNITY_EDITOR
        foreach (Transform child in newMapRoot)
            if (Application.isPlaying) Destroy(child.gameObject);
            else DestroyImmediate(child.gameObject);
#else
        foreach (Transform child in newMapRoot) Destroy(child.gameObject);
#endif
        mapRoot = newMapRoot;
        foreach (var row in nodeRows)
            foreach (var node in row)
                node.buttonUI = null;

        Vector2 startPos = mapRoot.GetComponent<RectTransform>().InverseTransformPoint(startAnchor.position);
        Vector2 endPos = mapRoot.GetComponent<RectTransform>().InverseTransformPoint(endAnchor.position);
        int levels = nodeRows.Count;

        for (int level = 0; level < levels; level++)
        {
            var rowList = nodeRows[level];
            int nodesInRow = rowList.Count;

            float levelProgress = levels > 1 ? (float)level / (levels - 1) : 0f;
            float xPos = Mathf.Lerp(startPos.x, endPos.x, levelProgress);

            for (int n = 0; n < nodesInRow; n++)
            {
                MapNode node = rowList[n];
                var go = Instantiate(nodePrefab, mapRoot);
                var button = go.GetComponent<MapNodeButton>();
                node.buttonUI = button;
                if (button != null)
                {
                    button.Set(node, normalColor, disabledColor);
                    button.button.onClick.RemoveAllListeners();
                    button.button.onClick.AddListener(() => button.OnClick());
                }

                var rt = go.GetComponent<RectTransform>();
                float totalColHeight = (nodesInRow - 1) * (endPos.y - startPos.y) / (nodesInRow > 1 ? nodesInRow - 1 : 1);
                float yOffset = n * totalColHeight / (nodesInRow > 1 ? nodesInRow - 1 : 1) - totalColHeight / 2f;
                float yPos = Mathf.Lerp(startPos.y, endPos.y, levelProgress) + yOffset;

                Vector2 finalPos = new Vector2(xPos, yPos);
                finalPos = ClampPositionToBoundingBox(finalPos);
                rt.anchoredPosition = finalPos;
            }
        }
        DrawAllConnections();
        UpdateNodeInteractability();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (mapAlreadyGenerated)
            RebuildMapUI(mapRoot);
        else
            GenerateMap();
    }

    MapNodeTypeSO PickWeightedNodeType(List<NodeTypeWeight> weights)
    {
        float total = 0;
        foreach (var w in weights) total += w.weight;
        float rand = Random.value * total;
        float running = 0;
        foreach (var w in weights)
        {
            running += w.weight;
            if (rand <= running) return w.nodeType;
        }
        return weights[0].nodeType;
    }

    public void SelectNode(MapNodeButton clickedButton)
    {
        if (!IsNodeSelectable(clickedButton.nodeData)) return;
        currentNode = clickedButton.nodeData;
    }

    public bool IsNodeSelectable(MapNode node)
    {
        if (currentNode == null) return node.levelIndex == 0;
        return node.levelIndex == currentNode.levelIndex + 1 && currentNode.connections.Contains(node);
    }

    void GenerateConnections()
    {
        for (int level = 0; level < nodeRows.Count - 1; level++)
        {
            var currLevel = nodeRows[level];
            var nextLevel = nodeRows[level + 1];
            foreach (var node in currLevel)
            {
                int minConns = 1;
                int maxConns = Mathf.Min(2, nextLevel.Count);
                int numConnections = Random.Range(minConns, maxConns + 1);
                HashSet<int> used = new();
                for (int c = 0; c < numConnections; c++)
                {
                    int idx;
                    do { idx = Random.Range(0, nextLevel.Count); }
                    while (!used.Add(idx) && used.Count < nextLevel.Count);
                    node.connections.Add(nextLevel[idx]);
                }
            }
            foreach (var nextNode in nextLevel)
            {
                bool isConnected = false;
                foreach (var node in currLevel)
                    if (node.connections.Contains(nextNode)) isConnected = true;
                if (!isConnected)
                {
                    int randIdx = Random.Range(0, currLevel.Count);
                    currLevel[randIdx].connections.Add(nextNode);
                }
            }
        }
    }

    public void UpdateNodeInteractability()
    {
        foreach (var row in nodeRows)
            foreach (var node in row)
                if (node.buttonUI != null)
                    node.buttonUI.SetInteractable(IsNodeSelectable(node));
    }

    void DrawAllConnections()
    {
        foreach (Transform child in mapRoot)
            if (child.name.StartsWith("Line_"))
#if UNITY_EDITOR
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
#else
                Destroy(child.gameObject);
#endif

        foreach (var row in nodeRows)
        {
            foreach (var node in row)
            {
                if (node.buttonUI == null) continue;
                foreach (var target in node.connections)
                {
                    if (target.buttonUI == null) continue;
                    GameObject lineGO = Instantiate(linePrefab, mapRoot);
                    lineGO.name = $"Line_{node.levelIndex}_{node.nodeIndex}_to_{target.levelIndex}_{target.nodeIndex}";
                    UILine uiLine = lineGO.GetComponent<UILine>();
                    uiLine.SetLine(
                        node.buttonUI.GetComponent<RectTransform>().anchoredPosition,
                        target.buttonUI.GetComponent<RectTransform>().anchoredPosition,
                        lineCutDistance,
                        lineThickness,
                        lineColor
                    );
                    lineGO.transform.SetAsFirstSibling();
                }
            }
        }
    }
}