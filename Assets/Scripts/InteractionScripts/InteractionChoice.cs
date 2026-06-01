using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractionChoice
{
    public string buttonText;
    public InteractionRequirement type;
    public int requirementValue;

    [Header("Potential Outcomes")]
    [Tooltip("If only 1 item, its 100% guaranteed. If multiple, then its a weighted chance")]
    public List<ChoiceOutcome> potentialOutcomes;
}

public enum InteractionRequirement { None, HPAmount, GoldAmount }
public enum InteractionEffect { None, ChangeHP, ChangeGold }