using UnityEngine;

[CreateAssetMenu(fileName = "DialogueCharacter", menuName = "Dialogue/Character")]
public class DialogueCharacterSO : ScriptableObject
{
    public string characterName;
    public Sprite characterPortrait;
    public Color dialogueColor = Color.white;
    [TextArea] public string characterDescription;
}