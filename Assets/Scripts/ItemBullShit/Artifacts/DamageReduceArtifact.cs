using UnityEngine;

[CreateAssetMenu(fileName = "DamageReduceArtifact", menuName = "Items/Artifacts/DamageReduce")]
public class DamageReduceArtifact : ArtifactSO
{
    [Range(0f, 1f)]
    public float reductionPercent = 0.2f;

    protected override void Subscribe()
    {
        GameEvents.OnDamageReceived += OnDamageReceived;
    }

    protected override void Unsubscribe()
    {
        GameEvents.OnDamageReceived -= OnDamageReceived;
    }

    private void OnDamageReceived(DamageEvent dmgEvent)
    {
        int reduced = Mathf.RoundToInt(dmgEvent.FinalDamage * (1f - reductionPercent));
        dmgEvent.ModifyDamage(reduced);
    }
}