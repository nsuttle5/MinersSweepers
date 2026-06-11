using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private RectTransform panelRect;
    [SerializeField] private TextMeshProUGUI itemNameTextBox;
    [SerializeField] private TextMeshProUGUI itemDescTextBox;

    [Header("Settings")]
    [SerializeField] private Vector2 offset = new(15f, -15f);

    private RectTransform rootRect;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        TryGetComponent(out rootRect);
        HideTooltip();
    }

    private void Update()
    {
        if (!tooltipPanel.activeSelf) return;

        UpdateTooltipPosition();
    }

    public void ShowTooltip(ItemSO item)
    {
        if (item == null) return;

        itemNameTextBox.text = item.itemName;
        itemDescTextBox.text = item.description;
        tooltipPanel.SetActive(true);

        Canvas.ForceUpdateCanvases();
        UpdateTooltipPosition();
    }

    public void HideTooltip() => tooltipPanel.SetActive(false);

    private void UpdateTooltipPosition()
    {
        if (Mouse.current == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();

        float panelWidth = panelRect.rect.width;

        float targetPivotX = 0f;
        float finalX = mousePos.x + offset.x;

        if (mousePos.x + panelWidth + offset.x > Screen.width)
        {
            targetPivotX = 1f;
            finalX = mousePos.x - offset.x;
        }

        float panelHeight = panelRect.rect.height;

        float finalY = mousePos.y + offset.y;

        float lowerBound = 0f;
        if (finalY < lowerBound) finalY = lowerBound;

        float upperBound = Screen.height - panelHeight;
        if (finalY > upperBound) finalY = upperBound;

        panelRect.pivot = new(targetPivotX, 0f);
        panelRect.position = new(finalX, finalY, 0f);
    }
}
