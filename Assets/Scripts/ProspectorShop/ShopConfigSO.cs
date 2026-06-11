using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ShopConfig", menuName = "Shop/ShopConfig")]
public class ShopConfigSO : ScriptableObject
{
    [Header("Slot Counts")]
    public int artifactSlotCount = 3;
    public int consumableSlotCount = 2;

    [Header("Reroll Settings")]
    public int baseRerollCost = 10;
    public int rerollCostIncrease = 5;

    [Header("Price Scaling Per Floor")]
    public float priceMultiplierPerFloor = 0.1f;

    [Header("Item Pools")]
    public List<ShopItemEntry> artifactPool;
    public List<ShopItemEntry> consumablePool;
}

[System.Serializable]
public class ShopItemEntry
{
    public ItemSO item;
    [Range(0f, 1f)] public float spawnWeight = 1f;
}