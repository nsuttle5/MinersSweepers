using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Components")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI quantityText;

    private ItemSO currentItem;

    public void Setup(ItemSO item, int quantity = 1)
    {
        currentItem = item;
        iconImage.sprite = item.icon;
        iconImage.enabled = item.icon != null;

        if (quantityText != null)
        {
            quantityText.gameObject.SetActive(quantity > 1);
            quantityText.text = $"x{quantity}";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;
        if (currentItem is not ConsumableSO consumable) return;

        consumable.OnUse();

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && TooltipUI.Instance != null)
            TooltipUI.Instance.ShowTooltip(currentItem);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.HideTooltip();
    }

    private void OnDisable()
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.HideTooltip();
    }
}
