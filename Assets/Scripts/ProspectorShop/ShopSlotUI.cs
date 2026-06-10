using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemDescriptionText;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI buyButtonText;
    [SerializeField] private GameObject soldOverlay;
    [SerializeField] private Image itemTypeIndicator;
    [SerializeField] private Color artifactColor = Color.yellow;
    [SerializeField] private Color consumableColor = Color.cyan;

    public ShopSlotData SlotData { get; private set; }

    public void Setup(ShopSlotData slot, Action onBuy)
    {
        SlotData = slot;

        itemIcon.sprite          = slot.item.icon;
        itemNameText.text        = slot.item.itemName;
        itemDescriptionText.text = slot.item.description;
        priceText.text           = $"{slot.finalPrice}g";
        buyButtonText.text       = "Buy";

        itemTypeIndicator.color = slot.item is ArtifactSO
            ? artifactColor
            : consumableColor;

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(() => onBuy());

        if (soldOverlay) soldOverlay.SetActive(false);
    }

    public void SetSold()
    {
        buyButton.interactable = false;
        buyButtonText.text     = "Sold";
        if (soldOverlay) soldOverlay.SetActive(true);
    }
}