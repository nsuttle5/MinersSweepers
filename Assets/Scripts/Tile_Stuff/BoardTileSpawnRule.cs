using UnityEngine;

[System.Serializable]
public class BoardTileSpawnRule
{
    public BoardTileSO tileType;
    public bool requiresEnemyUnderneath;
    public bool requiresEmptyUnderneath;
    [Range(0, 1)] public float spawnChance;
}
