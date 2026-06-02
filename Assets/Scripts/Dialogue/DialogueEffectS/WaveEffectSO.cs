using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "WaveEffect", menuName = "Dialogue/Effects/Wave")]
public class WaveEffectSO : DialogueEffectSO
{
    public float waveSpeed = 5f;
    public float waveAmount = 10f;
    public float duration = 2f;

    public override void ApplyEffect(TextMeshProUGUI textMesh, int characterIndex, float elapsedTime)
    {
        Debug.Log($"Wave Effect Called - Char Index: {characterIndex}, Elapsed Time: {elapsedTime}");

        var textInfo = textMesh.textInfo;
        if (characterIndex >= textInfo.characterCount)
        {
            Debug.Log($"Character index {characterIndex} >= character count {textInfo.characterCount}");
            return;
        }

        var charInfo = textInfo.characterInfo[characterIndex];
        if (!charInfo.isVisible)
        {
            Debug.Log($"Character {characterIndex} is not visible");
            return;
        }

        Debug.Log($"Applying wave to character {characterIndex}");

        float wave = Mathf.Sin(elapsedTime * waveSpeed + characterIndex * 0.5f) * waveAmount;

        int vertexIndex = charInfo.vertexIndex;
        int materialIndex = charInfo.materialReferenceIndex;

        Debug.Log($"Vertex Index: {vertexIndex}, Material Index: {materialIndex}, Wave Value: {wave}");

        var vertices = textInfo.meshInfo[materialIndex].vertices;

        for (int i = 0; i < 4; i++)
        {
            vertices[vertexIndex + i].y += wave;
        }

        textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.All);
    }

    public override float GetDuration() => duration;
}