using UnityEngine;

[CreateAssetMenu(fileName = "BeerMug",
    menuName = "Minesweeper/Spawnables/BeerMug")]
public class BeerMugSpawnableSO : SpawnableSO
{
    public int healAmount = 15;
    private void Awake() => type = SpawnableType.Gold;
}