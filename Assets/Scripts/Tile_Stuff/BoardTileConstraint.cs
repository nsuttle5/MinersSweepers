using UnityEngine;

[System.Serializable]
public class BoardTileConstraint
{
    public BoardTileSO tileType;
    public bool useRange = false;
    public int exactAmount = 1;
    public Vector2Int minMaxAmount = new Vector2Int(1, 3);
    public bool requiresEnemyUnderneath;
    public bool requiresEmptyUnderneath;
    [Range(0f, 1f)] public float spawnChance = 1f;

    public int GetQuantity()
    {
        if (useRange)
        {
            return Random.Range(minMaxAmount.x, minMaxAmount.y + 1);
        }
        else
        {
            return exactAmount;
        }
    }
}
