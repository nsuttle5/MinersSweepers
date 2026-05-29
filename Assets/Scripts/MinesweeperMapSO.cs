using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewMapLayout", menuName = "Minesweeper/Map Layout")]
public class MinesweeperMapSO : ScriptableObject
{
    public int width = 8;
    public int height = 8;
    public List<SpawnConstraint> spawnConstraints;
}