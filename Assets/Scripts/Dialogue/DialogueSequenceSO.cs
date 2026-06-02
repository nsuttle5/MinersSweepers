using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "DialogueSequence", menuName = "Dialogue/Sequence")]
public class DialogueSequenceSO : ScriptableObject
{
    public string sequenceID;
    [TextArea] public string description;
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("Timing Settings")]
    [SerializeField] public float typewriterSpeed = 0.05f;
    [SerializeField] public float fadeTransitionDuration = 0.5f;
    [SerializeField] public float linePauseDuration = 1.5f;

    public List<DialogueGroup> GetDialogueGroups()
    {
        var groups = new List<DialogueGroup>();

        if (dialogueLines.Count == 0) return groups;

        DialogueGroup currentGroup = new DialogueGroup();
        currentGroup.speaker = dialogueLines[0].speaker;
        currentGroup.lines = new List<DialogueLine> { dialogueLines[0] };

        for (int i = 1; i < dialogueLines.Count; i++)
        {
            if (dialogueLines[i].speaker == currentGroup.speaker)
            {
                currentGroup.lines.Add(dialogueLines[i]);
            }
            else
            {
                groups.Add(currentGroup);
                currentGroup = new DialogueGroup();
                currentGroup.speaker = dialogueLines[i].speaker;
                currentGroup.lines = new List<DialogueLine> { dialogueLines[i] };
            }
        }

        groups.Add(currentGroup);
        return groups;
    }
}

[System.Serializable]
public class DialogueGroup
{
    public DialogueCharacterSO speaker;
    public List<DialogueLine> lines = new List<DialogueLine>();

    public int GetLineCount() => lines.Count;
}