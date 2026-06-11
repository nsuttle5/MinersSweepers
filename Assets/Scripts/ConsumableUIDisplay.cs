using System.Collections.Generic;
using UnityEngine;

public class ConsumableUIDisplay : MonoBehaviour
{
    [SerializeField] private ItemSlotUI slotPrefab;
    [SerializeField] private Transform container;

    private void OnEnable()
    {
        GameEvents.OnConsumableObtained += HandleInventoryChanged;
        if (PlayerProfileManager.Instance) PlayerProfileManager.Instance.OnConsumablesChanged += HandleProfileUpdate;

        RefreshDisplay();
    }

    private void OnDisable()
    {
        GameEvents.OnConsumableObtained -= HandleInventoryChanged;
        if (PlayerProfileManager.HasInstance) PlayerProfileManager.Instance.OnConsumablesChanged -= HandleProfileUpdate;
    }

    private void HandleInventoryChanged(ConsumableSO _) => RefreshDisplay();

    private void HandleProfileUpdate(ConsumableSO _, int __) => RefreshDisplay();

    private void RefreshDisplay()
    {
        foreach (Transform child in container) Destroy(child.gameObject);

        if (!PlayerProfileManager.HasInstance) return;

        foreach (KeyValuePair<ConsumableSO, int> consumable in PlayerProfileManager.Instance.Consumables)
        {
            if (consumable.Value <= 0) continue;

            ItemSlotUI newSlot = Instantiate(slotPrefab, container);
            newSlot.Setup(consumable.Key, consumable.Value);
        }
    }
}
