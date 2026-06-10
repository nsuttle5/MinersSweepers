using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Minesweeper/Spawnables/Enemy")]
public class EnemySpawnableSO : SpawnableSO
{
    public Sprite interactedSprite;
    public bool isBoss = false;

    private void Awake() => type = SpawnableType.Enemy;
}
