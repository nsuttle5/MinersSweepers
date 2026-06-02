using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "Minesweeper/Spawnables/Enemy")]
public class EnemySpawnableSO : SpawnableSO
{
    public Sprite interactedSprite;

    private void Awake() => type = SpawnableType.Enemy;
}
