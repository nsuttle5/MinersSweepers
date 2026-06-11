public abstract class ConsumableSO : ItemSO
{
    public override void OnPurchase()
    {
        if (PlayerProfileManager.HasInstance) PlayerProfileManager.Instance.AddConsumable(this);
    }

    public override void OnUse()
    {
        if (!PlayerProfileManager.HasInstance) return;
        if (PlayerProfileManager.Instance.GetConsumableCount(this) <= 0) return;

        ApplyEffect();
        GameEvents.OnConsumableUsed?.Invoke(this);
        PlayerProfileManager.Instance.RemoveConsumable(this);
    }

    protected abstract void ApplyEffect();
}