using UnityEngine;

[CreateAssetMenu(fileName = "MapNodeType", menuName = "Map/NodeType")]
public class MapNodeTypeSO : ScriptableObject
{
    public string nodeTypeName;
    public Sprite nodeSprite;
    public string sceneToLoad;
    [TextArea] public string description;
}