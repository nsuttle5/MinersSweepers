using UnityEngine;

[CreateAssetMenu(fileName = "NewMole", menuName = "Minesweeper/Spawnables/Mole")]
public class MoleSpawnableSO : EnemySpawnableSO
{
    private void Awake() => type = SpawnableType.Enemy;
}