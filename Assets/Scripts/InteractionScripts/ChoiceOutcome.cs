using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChoiceOutcome
{
    [Range(0, 100)]
    public int weight = 50;

    [TextArea(3, 5)]
    public string outcomeText;

    public List<AppliedEffect> effects;

    public InteractionDataSO nextInterationStage;
}
