using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Transform artifactSlotsParent;
    [SerializeField] private Transform consumableSlotsParent;
    [SerializeField] private GameObject shopSlotPrefab;

    [Header("Reroll")]
    [SerializeField] private Button rerollButton;
    [SerializeField] private TextMeshProUGUI rerollCostText;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    private List<ShopSlotUI> _slotUIs = new();

    [Header("Player Info")]
    [SerializeField] private TextMeshProUGUI playerGoldText;

    private void Start()
    {
        rerollButton.onClick.AddListener(() => ShopManager.Instance.TryReroll());
        closeButton.onClick.AddListener(() => ShopManager.Instance.CloseShop());
        UpdateGoldDisplay(PlayerProfileManager.Instance.TotalGold);
        Hide();
    }

    public void Populate(List<ShopSlotData> slots, int rerollCost)
    {
        foreach (var slotUI in _slotUIs)
            Destroy(slotUI.gameObject);
        _slotUIs.Clear();

        foreach (var slot in slots)
        {
            Transform parent = slot.item is ArtifactSO
                ? artifactSlotsParent
                : consumableSlotsParent;

            GameObject go = Instantiate(shopSlotPrefab, parent);
            ShopSlotUI slotUI = go.GetComponent<ShopSlotUI>();
            slotUI.Setup(slot, () => ShopManager.Instance.TryPurchase(slot));
            _slotUIs.Add(slotUI);
        }

        UpdateRerollCost(rerollCost);
    }

    public void MarkSlotSold(ShopSlotData slot)
    {
        foreach (var slotUI in _slotUIs)
            if (slotUI.SlotData == slot)
                slotUI.SetSold();
    }

    public void UpdateRerollCost(int cost)
    {
        rerollCostText.text = $"Reroll: {cost}g";
    }

    public void Show()
    {
        shopPanel.SetActive(true);
    }

    public void Hide()
    {
        shopPanel.SetActive(false);
    }

    private void OnEnable()
    {
        PlayerProfileManager.Instance.OnGlobalGoldChanged += UpdateGoldDisplay;
    }

    private void OnDisable()
    {
        if (PlayerProfileManager.HasInstance)
            PlayerProfileManager.Instance.OnGlobalGoldChanged -= UpdateGoldDisplay;
    }

    private void UpdateGoldDisplay(int newGold)
    {
        if (playerGoldText)
            playerGoldText.text = $"Gold: {newGold}g";
    }
}