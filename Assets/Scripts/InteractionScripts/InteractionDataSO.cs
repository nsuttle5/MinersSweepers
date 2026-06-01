using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewInteraction", menuName = "Interactions/InteractionData")]
public class InteractionDataSO : ScriptableObject
{
    public string interactionID;
    public string interactionTitle;
    [TextArea(3, 10)] public string initialDescription;
    public Sprite interactionArtwork;

    public List<InteractionChoice> choices;
}