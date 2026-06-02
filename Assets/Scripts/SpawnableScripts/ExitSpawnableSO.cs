using UnityEngine;

[CreateAssetMenu(fileName = "NewExit", menuName = "Minesweeper/Spawnables/Exit")]
public class ExitSpawnableSO : SpawnableSO
{
    private void Awake() => type = SpawnableType.Exit;
}
