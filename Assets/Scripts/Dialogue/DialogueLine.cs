using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueLine
{
    public DialogueCharacterSO speaker;
    [TextArea] public string text;

    [Header("Text Effects")]
    public List<TextEffectSegment> textEffects = new List<TextEffectSegment>();
}

[System.Serializable]
public class TextEffectSegment
{
    public int startCharacterIndex;
    public int endCharacterIndex;
    public DialogueEffectSO effect;
}