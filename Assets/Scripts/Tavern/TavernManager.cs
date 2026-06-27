using UnityEngine.Events;

public static class TavernManager
{
    public static UnityAction OnStateChanged;

    public static int GetUpgradeLevel(TavernUpgradeType upgradeType)
    {
        return upgradeType switch
        {
            TavernUpgradeType.Health => PlayerProfileManager.Instance.HpUpgradeLevel,
            TavernUpgradeType.Luck => PlayerProfileManager.Instance.LuckUpgradeLevel,
            TavernUpgradeType.Gold_Gain => PlayerProfileManager.Instance.GoldGainUpgradeLevel,
            TavernUpgradeType.Consumable_Slot => PlayerProfileManager.Instance.ConsumableSlotUpgradeLevel,
            TavernUpgradeType.Initial_Radius => PlayerProfileManager.Instance.InitialRadiusUpgradeLevel,
            TavernUpgradeType.Shop_Rerolls => PlayerProfileManager.Instance.ShopRerollsUpgradeLevel,
            _ => 0
        };
    }

    public static bool TryPurchaseUpgrade(TavernUpgradeType upgradeType, int cost, int maxLevel = -1)
    {
        int currentLevel = GetUpgradeLevel(upgradeType);
        if (maxLevel >= 0 && currentLevel >= maxLevel) return false;

        if (!PlayerProfileManager.Instance.TrySpendGold(cost)) return false;

        SetUpgradeLevel(upgradeType, currentLevel + 1);
        OnStateChanged?.Invoke();
        return true;
    }

    private static void SetUpgradeLevel(TavernUpgradeType upgradeType, int level)
    {
        switch (upgradeType)
        {
            case TavernUpgradeType.Health:
                PlayerProfileManager.Instance.HpUpgradeLevel = level;
                break;
            case TavernUpgradeType.Luck:
                PlayerProfileManager.Instance.LuckUpgradeLevel = level;
                break;
            case TavernUpgradeType.Gold_Gain:
                PlayerProfileManager.Instance.GoldGainUpgradeLevel = level;
                break;
            case TavernUpgradeType.Consumable_Slot:
                PlayerProfileManager.Instance.ConsumableSlotUpgradeLevel = level;
                break;
            case TavernUpgradeType.Initial_Radius:
                PlayerProfileManager.Instance.InitialRadiusUpgradeLevel = level;
                break;
            case TavernUpgradeType.Shop_Rerolls:
                PlayerProfileManager.Instance.ShopRerollsUpgradeLevel = level;
                break;
        }
    }
}

public enum TavernUpgradeType
{
    Health,
    Luck,
    Gold_Gain,
    Consumable_Slot,
    Initial_Radius,
    Shop_Rerolls
}
