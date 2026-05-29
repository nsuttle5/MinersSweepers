using UnityEngine;
using System.Collections.Generic;

public class MapManager : MonoBehaviour
{
    public MapGenConfigSO mapConfig;
    public List<MapNodeTypeSO> allNodeTypes;
    public Transform mapRoot;

    [Header("Node Prefab")]
    public GameObject nodePrefab;

    private List<List<MapNode>> nodeRows = new List<List<MapNode>>();

    [ContextMenu("GenerateMap")]
    public void GenerateMap()
    {
#if UNITY_EDITOR
        foreach (Transform child in mapRoot)
            if (Application.isPlaying)
                Destroy(child.gameObject);
            else
                DestroyImmediate(child.gameObject);
#else
        foreach (Transform child in mapRoot)
            Destroy(child.gameObject);
#endif

        nodeRows.Clear();

        int levels = mapConfig.numLevels;
        float ySpacing = 150f;
        float xSpacing = 150f;

        for (int level = 0; level < levels; level++)
        {
            int nodesInRow = mapConfig.nodesPerLevel[level];
            var weightsData = mapConfig.weightsPerLevel.Find(w => w.level == level);
            List<NodeTypeWeight> weights = (weightsData != null && weightsData.nodeWeights.Count > 0)
                ? weightsData.nodeWeights
                : mapConfig.weightsPerLevel[0].nodeWeights;

            var rowList = new List<MapNode>();

            for (int n = 0; n < nodesInRow; n++)
            {
                MapNode node = new MapNode();
                node.levelIndex = level;
                node.nodeIndex = n;
                node.type = PickWeightedNodeType(weights);
                rowList.Add(node);

                var go = Instantiate(nodePrefab, mapRoot);
                var button = go.GetComponent<MapNodeButton>();
                node.buttonUI = button;
                if (button != null)
                {
                    button.Set(node);
                }

                RectTransform rt = go.GetComponent<RectTransform>();
                float x = level * xSpacing;
                float totalColHeight = (nodesInRow - 1) * ySpacing;
                float y = n * ySpacing - totalColHeight / 2f;
                rt.anchoredPosition = new Vector2(x, y);
            }
            nodeRows.Add(rowList);
        }

        for (int level = 0; level < nodeRows.Count - 1; level++)
        {
            foreach (var node in nodeRows[level])
            {
                var nextRow = nodeRows[level + 1];
                int numConnections = Random.Range(1, 3);
                for (int c = 0; c < numConnections; c++)
                {
                    int idx = Random.Range(0, nextRow.Count);
                    if (!node.connections.Contains(nextRow[idx]))
                        node.connections.Add(nextRow[idx]);
                }
            }
        }
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
            if (rand <= running)
                return w.nodeType;
        }
        return weights[0].nodeType;
    }
}