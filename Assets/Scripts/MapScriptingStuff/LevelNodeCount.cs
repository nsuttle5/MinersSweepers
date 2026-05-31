using UnityEngine;

[System.Serializable]
public struct LevelNodeCount
{
    public bool useRange;
    public int setNumber;
    public Vector2Int range;

    public int GetValue()
    {
        if (useRange)
            return Random.Range(range.x, range.y + 1);
        return setNumber;
    }
}