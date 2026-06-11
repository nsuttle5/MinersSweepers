using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerProfileManager : MonoBehaviour
{
    #region Instance
    private static PlayerProfileManager _instance;
    public static PlayerProfileManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerProfileManager>();
                if (_instance == null)
                {
                    GameObject gameDataGO = new("ProfileManager");
                    _instance = gameDataGO.AddComponent<PlayerProfileManager>();
                }
            }
            return _instance;
        }
    }
    public static bool HasInstance => _instance != null;
    #endregion

    public int TotalGold { get; set; } = 0;
    public int HpUpgradeLevel { get; set; } = 1;
    public int LuckUpgradeLevel { get; set; } = 1;

    private Dictionary<ConsumableSO, int> consumables;
    public IReadOnlyDictionary<ConsumableSO, int> Consumables => consumables;

    public UnityAction<int> OnGlobalGoldChanged;
    public UnityAction<ConsumableSO, int> OnConsumablesChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        consumables = new();
    }

    public void AddGoldToWallet(int amount)
    {
        TotalGold = Mathf.Max(0, TotalGold + amount);
        OnGlobalGoldChanged?.Invoke(TotalGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (TotalGold >= amount)
        {
            AddGoldToWallet(-amount);
            return true;
        }
        return false;
    }

    public void AddConsumable(ConsumableSO consumable)
    {
        if (!consumables.ContainsKey(consumable)) consumables[consumable] = 0;
        consumables[consumable]++;
        GameEvents.OnConsumableObtained?.Invoke(consumable);
        OnConsumablesChanged?.Invoke(consumable, consumables[consumable]);
    }

    public void RemoveConsumable(ConsumableSO consumable)
    {
        if (consumables.ContainsKey(consumable) && consumables[consumable] > 0)
        {
            consumables[consumable]--;
            OnConsumablesChanged?.Invoke(consumable, consumables[consumable]);
        }
    }

    public int GetConsumableCount(ConsumableSO consumable) =>
        consumables.TryGetValue(consumable, out int count) ? count : 0;
}
