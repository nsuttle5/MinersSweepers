using UnityEngine;
using UnityEngine.UI;
using TMPro;

public abstract class DialogueEffectSO : ScriptableObject
{
    public abstract void ApplyEffect(TextMeshProUGUI textMesh, int characterIndex, float elapsedTime);
    public abstract float GetDuration();
}