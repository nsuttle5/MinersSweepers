using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(fileName = "FishingConfig", menuName = "Fishing/FishingConfig")]
public class FishingConfigSO : ScriptableObject
{
    [Header("Column Settings")]
    public int columnCount = 3;
    public float cellSize = 1f;

    [Header("Scroll Settings")]
    public float initialScrollSpeed = 1f;
    public float maxScrollSpeed = 5f;
    public float speedIncreaseRate = 0.1f;

    [Header("Row Settings")]
    public int visibleRowBuffer = 20;

    [Header("Difficulty Tiers")]
    public List<FishingDifficultyTier> difficultyTiers;
}

[System.Serializable]
public class FishingDifficultyTier
{
    public float timeThreshold;
    [Range(0f, 1f)] public float bombChance;
    [Range(0f, 1f)] public float fishChance;
}
