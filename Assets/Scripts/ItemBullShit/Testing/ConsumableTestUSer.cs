using UnityEngine;
using UnityEngine.InputSystem;

public class ConsumableTestUser : MonoBehaviour
{
    [SerializeField] private ConsumableSO consumableToTest;

    private void Update()
    {
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            ArtifactManager.Instance.UseConsumable(consumableToTest);
            Debug.Log("Used consumable: " + consumableToTest?.itemName);
        }

        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            Debug.Log("Artifacts held: " + ArtifactManager.Instance.Artifacts.Count);
            foreach (var a in ArtifactManager.Instance.Artifacts)
                Debug.Log(" - " + a.itemName);

            Debug.Log("Consumables held: " + ArtifactManager.Instance.Consumables.Count);
            foreach (var c in ArtifactManager.Instance.Consumables)
                Debug.Log(" - " + c.itemName);
        }
    }
}