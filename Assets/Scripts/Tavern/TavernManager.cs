using System;
using UnityEngine;

public class TavernManager : MonoBehaviour
{
    public static TavernManager Instance { get; private set; }

    [SerializeField] private int gold;

    [Header("Permanent Upgrades")]
    [SerializeField] private int damageUpgradeLevel;
    [SerializeField] private int healthUpgradeLevel;
    [SerializeField] private int luckUpgradeLevel;

    public event Action OnStateChanged;

    public int Gold => gold;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        gold += amount;
        OnStateChanged?.Invoke();
    }

    public bool CanAfford(int amount)
    {
        return amount >= 0 && gold >= amount;
    }

    public bool TrySpendGold(int amount)
    {
        if (!CanAfford(amount)) return false;

        gold -= amount;
        OnStateChanged?.Invoke();
        return true;
    }

    public int GetUpgradeLevel(TavernUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            TavernUpgradeType.Damage => damageUpgradeLevel,
            TavernUpgradeType.Health => healthUpgradeLevel,
            TavernUpgradeType.Luck => luckUpgradeLevel,
            _ => 0
        };
    }

    public bool TryPurchaseUpgrade(TavernUpgradeType upgradeType, int cost, int maxLevel = -1)
    {
        int currentLevel = GetUpgradeLevel(upgradeType);
        if (maxLevel >= 0 && currentLevel >= maxLevel)
        {
            return false;
        }

        if (!TrySpendGold(cost))
        {
            return false;
        }

        SetUpgradeLevel(upgradeType, currentLevel + 1);
        OnStateChanged?.Invoke();
        return true;
    }

    private void SetUpgradeLevel(TavernUpgradeType upgradeType, int level)
    {
        switch (upgradeType)
        {
            case TavernUpgradeType.Damage:
                damageUpgradeLevel = level;
                break;
            case TavernUpgradeType.Health:
                healthUpgradeLevel = level;
                break;
            case TavernUpgradeType.Luck:
                luckUpgradeLevel = level;
                break;
        }
    }
}

public enum TavernUpgradeType
{
    Damage,
    Health,
    Luck
}
