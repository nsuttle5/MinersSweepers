using UnityEngine;

[CreateAssetMenu(fileName = "NewGold", menuName = "Minesweeper/Spawnables/Gold")]
public class GoldSpawnableSO : SpawnableSO
{
    public int goldValue;

    private void Awake() => type = SpawnableType.Gold;
}
