using UnityEngine;

public enum ItemType { Consumable, Artifact }

public abstract class ItemSO : ScriptableObject
{
    [Header("Item Info")]
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Shop")]
    public int goldCost;

    [Header("Type")]
    public ItemType itemType;

    public abstract void OnPurchase(); // Item purchased
    public virtual void OnUse() => Debug.Log($"{itemName} used!"); // Consumable Used
    public virtual void OnRunStart() => Debug.Log($"{itemName} run started!");
    public virtual void OnRunEnd() => Debug.Log($"{itemName} run ended!");
    public virtual void OnUpdate() { }
}