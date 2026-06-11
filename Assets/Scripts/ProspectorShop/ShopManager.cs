using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance { get; private set; }

    [Header("Config")]
    [SerializeField] private ShopConfigSO config;

    [Header("UI")]
    [SerializeField] private ShopUI shopUI;

    private List<ShopSlotData> _currentSlots = new();
    private int _currentRerollCost;
    private int _currentFloor = 0;

    public static UnityAction OnShopRerolled;
    public static UnityAction OnShopOpened;
    public static UnityAction OnShopClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        _currentRerollCost = config.baseRerollCost;
        _currentFloor = MapManager.Instance != null ? GetCurrentFloor() : 0;
        GenerateShop();
        OpenShop();
    }

    private int GetCurrentFloor()
    {
        //return MapCameraController.GetSavedLevelIndex();
        return 0;
    }

    public void GenerateShop()
    {
        _currentSlots.Clear();

        HashSet<ItemSO> usedItems = new HashSet<ItemSO>();

        for (int i = 0; i < config.artifactSlotCount; i++)
        {
            ItemSO item = PickWeightedItem(config.artifactPool, usedItems);
            if (item != null)
            {
                usedItems.Add(item);
                _currentSlots.Add(new ShopSlotData(item, GetScaledPrice(item)));
            }
        }

        for (int i = 0; i < config.consumableSlotCount; i++)
        {
            ItemSO item = PickWeightedItem(config.consumablePool, usedItems);
            if (item != null)
            {
                usedItems.Add(item);
                _currentSlots.Add(new ShopSlotData(item, GetScaledPrice(item)));
            }
        }

        shopUI.Populate(_currentSlots, _currentRerollCost);
    }

    public void TryPurchase(ShopSlotData slot)
    {
        if (slot.isSold) return;
        if (!PlayerProfileManager.Instance.TrySpendGold(slot.finalPrice)) return;

        GameEvents.OnGoldSpent?.Invoke(slot.finalPrice);
        slot.item.OnPurchase();
        slot.isSold = true;

        GameEvents.OnItemPurchased?.Invoke(slot); // General event for any item purchase
        if (slot.item is ArtifactSO artifact)
            GameEvents.OnArtifactPurchased?.Invoke(artifact); // Specific event for artifact purchase
        else if (slot.item is ConsumableSO consumable)
            GameEvents.OnConsumablePurchased?.Invoke(consumable); // Specific event for consumable purchase

        int remaining = _currentSlots.FindAll(s => !s.isSold).Count;
        GameEvents.OnShopItemsRemaining?.Invoke(remaining); // Notify about remaining items

        shopUI.MarkSlotSold(slot);
    }

    public void TryReroll()
    {
        if (!PlayerProfileManager.Instance.TrySpendGold(_currentRerollCost)) return;

        GameEvents.OnGoldSpent?.Invoke(_currentRerollCost);
        _currentRerollCost += config.rerollCostIncrease;

        GenerateShop();
        GameEvents.OnShopRerolled?.Invoke(_currentRerollCost); // Notify about reroll with new cost
        OnShopRerolled?.Invoke();
    }

    private int GetScaledPrice(ItemSO item)
    {
        float multiplier = 1f + (_currentFloor * config.priceMultiplierPerFloor);
        return Mathf.RoundToInt(item.goldCost * multiplier);
    }

    private ItemSO PickWeightedItem(List<ShopItemEntry> pool, HashSet<ItemSO> exclude)
    {
        if (pool == null || pool.Count == 0) return null;

        List<ShopItemEntry> available = pool.FindAll(e => !exclude.Contains(e.item));
        if (available.Count == 0) return null;

        float total = 0f;
        foreach (var entry in available) total += entry.spawnWeight;

        float roll = Random.value * total;
        float running = 0f;
        foreach (var entry in available)
        {
            running += entry.spawnWeight;
            if (roll <= running) return entry.item;
        }

        return available[available.Count - 1].item;
    }

    public void OpenShop()
    {
        shopUI.Show();
        GameEvents.OnShopOpened?.Invoke(); // Notify about shop opening
        OnShopOpened?.Invoke();
    }

    public void CloseShop()
    {
        shopUI.Hide();
        GameEvents.OnShopClosed?.Invoke(); // Notify about shop closing
        OnShopClosed?.Invoke();

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("MapTesting");
        else
            UnityEngine.SceneManagement.SceneManager.LoadScene("MapTesting");
    }
}

public class ShopSlotData
{
    public ItemSO item;
    public int finalPrice;
    public bool isSold;

    public ShopSlotData(ItemSO item, int price)
    {
        this.item = item;
        this.finalPrice = price;
        this.isSold = false;
    }
}