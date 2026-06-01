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

    ///called when the item is purchased and added to inventory
    public virtual void OnPurchase()
    {
        Debug.Log($"{itemName} purchased!");
    }

    ///called when a consumable item is used
    public virtual void OnUse()
    {
        Debug.Log($"{itemName} used!");
    }

    public virtual void OnRunStart()
    {
        Debug.Log($"{itemName} run started!");
    }

    public virtual void OnRunEnd()
    {
        Debug.Log($"{itemName} run ended!");
    }

    ///called every frame for continuous effects on artifacts
    public virtual void OnUpdate()
    {
    }
}