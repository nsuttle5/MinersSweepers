using UnityEngine;

[CreateAssetMenu(fileName = "GoldOnKillArtifact", menuName = "Items/Artifacts/GoldOnKill")]
public class GoldOnKillArtifact : ArtifactSO
{
    public int goldPerKill = 5;

    protected override void Subscribe()
    {
        GameEvents.OnEnemyDefeated += OnEnemyDefeated;
    }

    protected override void Unsubscribe()
    {
        GameEvents.OnEnemyDefeated -= OnEnemyDefeated;
    }

    private void OnEnemyDefeated(CellView cell)
    {
        GameData.Instance.CollectGold(goldPerKill);
    }
}