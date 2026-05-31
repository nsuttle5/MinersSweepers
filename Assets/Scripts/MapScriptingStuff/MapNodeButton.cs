using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MapNodeButton : MonoBehaviour
{
    [Header("UI References")]
    public Image bgImage;
    public Image nodeIconImage;
    public Button button;

    [HideInInspector] public MapNode nodeData;

    Color _normal = Color.white;
    Color _disabled = Color.gray;

    public void Set(MapNode node, Color normal, Color disabled)
    {
        nodeData = node;
        _normal = normal;
        _disabled = disabled;
        if (nodeIconImage != null)
            nodeIconImage.sprite = node.type.nodeSprite;
    }

    public void OnClick()
    {
        if (nodeData != null && nodeData.type != null && !string.IsNullOrEmpty(nodeData.type.sceneToLoad))
        {
            MapManager.Instance.SelectNode(this);
            SceneManager.LoadScene(nodeData.type.sceneToLoad);
        }
    }

    public void SetInteractable(bool interactable)
    {
        if (button != null)
            button.interactable = interactable;
        if (bgImage != null)
            bgImage.color = interactable ? _normal : _disabled;
    }
}