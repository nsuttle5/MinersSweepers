using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MapNodeButton : MonoBehaviour
{
    public Image bgImage;
    public Image nodeIconImage;
    public Button button;

    private MapNode nodeData;

    public void Set(MapNode node)
    {
        nodeData = node;
        if (nodeIconImage != null)
            nodeIconImage.sprite = node.type.nodeSprite;

    }
}