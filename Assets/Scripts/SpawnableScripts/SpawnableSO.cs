using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewSpawnable", menuName = "Minesweeper/Spawnable")]
public abstract class SpawnableSO : ScriptableObject
{
    public SpawnableType type;
    public string displayName;
    public Sprite sprite;
    public int damage;
    public int health;

    [TextArea(2,5)] public string description;
    public string abilityTooltip;
    public List<SpawnableAbilitiesSO> abilities;
}

public enum SpawnableType { Enemy, Gold, Exit }