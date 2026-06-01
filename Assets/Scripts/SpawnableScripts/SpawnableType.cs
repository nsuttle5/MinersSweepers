using UnityEngine;
using System.Collections.Generic;

public enum SpawnableType { Enemy, Gold, Exit }

[CreateAssetMenu(fileName = "NewSpawnable", menuName = "Minesweeper/Spawnable")]
public class SpawnableSO : ScriptableObject
{
    public SpawnableType type;
    public string displayName;
    public Sprite sprite;
    public int damage;
    public int health;

    public string description;
    public string abilityTooltip;
    public List<SpawnableAbilitiesSO> abilities;

}