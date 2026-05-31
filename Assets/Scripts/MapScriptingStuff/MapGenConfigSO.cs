using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapGenConfig", menuName = "Map/GenerationConfig")]
public class MapGenConfigSO : ScriptableObject
{
    [Header("Levels")]
    public int numLevels = 5;

    [Header("Nodes Per Level")]
    public List<LevelNodeCount> nodesPerLevel = new List<LevelNodeCount>();

    [System.Serializable]
    public class NodeWeightsForLevel
    {
        public int level;
        public List<NodeTypeWeight> nodeWeights;
    }
    public List<NodeWeightsForLevel> weightsPerLevel;
}