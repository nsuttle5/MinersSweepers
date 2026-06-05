using UnityEngine;

[CreateAssetMenu(fileName = "NewMoleHole", menuName = "Minesweeper/Spawnables/MoleHole")]
public class MoleHoleSpawnableSO : SpawnableSO
{
    private void Awake() => type = SpawnableType.Enemy;
}