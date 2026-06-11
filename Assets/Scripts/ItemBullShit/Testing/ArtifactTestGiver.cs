using UnityEngine;

public class ArtifactTestGiver : MonoBehaviour
{
    [SerializeField] private ArtifactSO[] artifactsToGive;
    [SerializeField] private ConsumableSO[] consumablesToGive;

    private void Start()
    {
        foreach (var artifact in artifactsToGive)
            PlayerRunStats.Instance.AddArtifact(artifact);

        foreach (var consumable in consumablesToGive)
            PlayerProfileManager.Instance.AddConsumable(consumable);
    }
}