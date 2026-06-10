using UnityEngine.Events;

public static class GameEvents
{
    //Damage
    public static UnityAction<DamageEvent> OnDamageReceived;
    public static UnityAction<int> OnPlayerHealed;
    public static UnityAction OnPlayerDeath;

    //Gold
    public static UnityAction<GoldEvent> OnGoldCollected;
    public static UnityAction<int> OnGoldSpent;

    //Enemies
    public static UnityAction<CellView> OnEnemyDefeated;
    public static UnityAction<int> OnEnemyDefeatedCountReached;
    public static UnityAction<CellView> OnEnemyRevealed;
    public static UnityAction<CellView> OnBossDefeated;

    //Cells
    public static UnityAction<CellView> OnCellRevealed;
    public static UnityAction<int> OnCellRevealedCountReached;
    public static UnityAction<CellView> OnCellHidden;
    public static UnityAction<CellView> OnEmptyCellRevealed;
    public static UnityAction<CellView> OnGoldCellRevealed;
    public static UnityAction<CellView> OnExitRevealed;
    public static UnityAction<CellView> OnMineRevealed;

    //Board
    public static UnityAction OnBoardGenerated;
    public static UnityAction OnFirstCellRevealed;
    public static UnityAction<float> OnBoardPercentRevealed;

    //Player State
    public static UnityAction<int, int> OnHealthChanged;
    public static UnityAction OnLowHealth;
    public static UnityAction OnFullHealth;

    //Items
    public static UnityAction<ArtifactSO> OnArtifactObtained;
    public static UnityAction<ConsumableSO> OnConsumableUsed;
    public static UnityAction<ConsumableSO> OnConsumableObtained;

    //Run
    public static UnityAction OnRunStart;
    public static UnityAction OnRunEnd;
    public static UnityAction OnFloorComplete;
    public static UnityAction OnExitUsed;
}

public class DamageEvent
{
    public int RawDamage { get; }
    public int FinalDamage { get; private set; }

    public DamageEvent(int damage)
    {
        RawDamage = damage;
        FinalDamage = damage;
    }

    public void ModifyDamage(int newDamage) => FinalDamage = newDamage;
}

public class GoldEvent
{
    public int RawAmount { get; }
    public int FinalAmount { get; private set; }

    public GoldEvent(int amount)
    {
        RawAmount = amount;
        FinalAmount = amount;
    }

    public void ModifyAmount(int newAmount) => FinalAmount = newAmount;
}