using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "MapGenConfig", menuName = "Map/GenerationConfig")]
public class MapGenConfigSO : ScriptableObject
{
    public int numLevels = 5;
    public List<int> nodesPerLevel = new List<int> { 1, 2, 3, 2, 1 };

    [System.Serializable]
    public class NodeWeightsForLevel
    {
        public int level;
        public List<NodeTypeWeight> nodeWeights;
    }
    public List<NodeWeightsForLevel> weightsPerLevel;
}