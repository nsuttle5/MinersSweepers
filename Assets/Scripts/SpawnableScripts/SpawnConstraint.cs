using UnityEngine;

[System.Serializable]
public class SpawnConstraint
{
    public SpawnableSO spawnable;
    public bool useRange = false;
    public int exactAmount = 1;
    public Vector2Int minMaxAmount = new Vector2Int(1, 5);
    public SpawnPriority priority = SpawnPriority.Medium;

    public int GetQuantity()
    {
        if (useRange) return Random.Range(minMaxAmount.x, minMaxAmount.y + 1);
        else return exactAmount;
    }
}