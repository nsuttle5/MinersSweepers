using UnityEngine;

public abstract class ArtifactSO : ItemSO
{
    public override void OnPurchase()
    {
        if (PlayerRunStats.HasInstance) PlayerRunStats.Instance.AddArtifact(this);
    }

    public virtual void OnObtain() => Subscribe();
    public virtual void OnRemove() => Unsubscribe();

    protected virtual void Subscribe() { }
    protected virtual void Unsubscribe() { }
}
