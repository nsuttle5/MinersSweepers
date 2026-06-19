using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Characters/Character")]
public class CharacterSO : ScriptableObject
{
    [Header("Display")]
    public string characterName;
    [TextArea] public string description;
    public Sprite icon;
    public Sprite portrait;

    [Header("Stats")]
    public int startingMaxHP = 100;

    [Header("Starting Loadout")]
    public List<ArtifactSO> startingArtifacts;
    public List<ConsumableSO> startingConsumables;

    [Header("Passive Abilities")]
    public List<ArtifactSO> characterPassives;
}
