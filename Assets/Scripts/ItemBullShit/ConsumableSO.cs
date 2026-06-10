public abstract class ConsumableSO : ItemSO
{
    public override void OnPurchase()
    {
        ArtifactManager.Instance.AddConsumable(this);
    }

    public override void OnUse()
    {
        ApplyEffect();
        GameEvents.OnConsumableUsed?.Invoke(this);
        ArtifactManager.Instance.RemoveConsumable(this);
    }

    protected abstract void ApplyEffect();
}