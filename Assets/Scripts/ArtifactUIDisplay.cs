using UnityEngine;

public class ArtifactUIDisplay : MonoBehaviour
{
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private Transform container;

    private void OnEnable()
    {
        GameEvents.OnArtifactObtained += RefreshDisplay;
        RefreshDisplay(null);
    }

    private void OnDisable()
    {
        GameEvents.OnArtifactObtained -= RefreshDisplay;
    }

    private void RefreshDisplay(ArtifactSO _)
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        if (ArtifactManager.Instance == null) return;

        foreach (ArtifactSO artifact in ArtifactManager.Instance.Artifacts)
        {
            ItemSlotUI newSlot = Instantiate(slotPrefab, container);
            newSlot.Setup(artifact);
        }
    }
}