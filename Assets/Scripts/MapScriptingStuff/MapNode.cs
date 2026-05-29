using System.Collections.Generic;

public class MapNode
{
    public int levelIndex;
    public int nodeIndex;
    public MapNodeTypeSO type;
    public List<MapNode> connections = new List<MapNode>();

    public MapNodeButton buttonUI;
}