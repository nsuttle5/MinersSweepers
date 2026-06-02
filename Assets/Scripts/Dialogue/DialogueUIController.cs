using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class DialogueUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private Image characterPortrait;
    [SerializeField] private Button skipButton;
    [SerializeField] private CanvasGroup dialogueCanvasGroup;

    private DialogueSequenceSO currentSequence;
    private bool isPlaying = false;

    public static DialogueUIController Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    private void Start()
    {
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipDialogue);
        }

        if (dialogueCanvasGroup != null)
        {
            dialogueCanvasGroup.alpha = 0;
        }
    }

    private void OnDestroy()
    {
        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipDialogue);
        }
    }

    public void StartDialogueSequence(DialogueSequenceSO sequence)
    {
        if (sequence == null)
        {
            Debug.LogError("Dialogue sequence is null!");
            return;
        }

        if (isPlaying)
        {
            Debug.LogWarning("Dialogue already playing!");
            return;
        }

        currentSequence = sequence;
        isPlaying = true;
        StartCoroutine(DialogueSequenceRoutine());
    }

    private IEnumerator DialogueSequenceRoutine()
    {
        var groups = currentSequence.GetDialogueGroups();

        for (int groupIndex = 0; groupIndex < groups.Count; groupIndex++)
        {
            if (!isPlaying) break;

            var group = groups[groupIndex];

            if (group.speaker != null)
            {
                speakerNameText.text = group.speaker.characterName;
                speakerNameText.color = group.speaker.dialogueColor;
                characterPortrait.sprite = group.speaker.characterPortrait;
            }

            yield return StartCoroutine(FadeIn());

            for (int i = 0; i < group.lines.Count; i++)
            {
                if (!isPlaying) break;

                yield return StartCoroutine(DisplayLine(group.lines[i]));

                if (i < group.lines.Count - 1)
                {
                    yield return new WaitForSeconds(currentSequence.linePauseDuration);
                }
                else if (groupIndex == groups.Count - 1)
                {
                    yield return new WaitForSeconds(currentSequence.linePauseDuration);
                }
            }

            yield return StartCoroutine(FadeOut());

            if (groupIndex < groups.Count - 1)
            {
                yield return new WaitForSeconds(0.3f);
            }
        }

        isPlaying = false;
    }

    private IEnumerator DisplayLine(DialogueLine line)
    {
        yield return StartCoroutine(TypewriterEffect(line.text, line.textEffects));
    }

    private IEnumerator TypewriterEffect(string text, List<TextEffectSegment> effects)
    {
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i < text.Length; i++)
        {
            if (!isPlaying) break;

            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(currentSequence.typewriterSpeed);
        }

        dialogueText.maxVisibleCharacters = text.Length;
        dialogueText.ForceMeshUpdate();
    }

    private void Update()
    {
        if (!isPlaying || currentSequence == null) return;

        var groups = currentSequence.GetDialogueGroups();
        if (groups.Count > 0)
        {
            for (int groupIdx = 0; groupIdx < groups.Count; groupIdx++)
            {
                for (int lineIdx = 0; lineIdx < groups[groupIdx].lines.Count; lineIdx++)
                {
                    var line = groups[groupIdx].lines[lineIdx];
                    UpdateTextEffects(line.textEffects);
                }
            }
        }
    }

    private void UpdateTextEffects(List<TextEffectSegment> effects)
    {
        foreach (var segment in effects)
        {
            if (segment.effect == null) continue;

            for (int i = segment.startCharacterIndex; i <= segment.endCharacterIndex; i++)
            {
                segment.effect.ApplyEffect(dialogueText, i, Time.time);
            }
        }

        dialogueText.ForceMeshUpdate();
    }
    private IEnumerator FadeIn()
    {
        if (dialogueCanvasGroup == null) yield break;

        dialogueCanvasGroup.alpha = 0;
        float elapsed = 0;

        while (elapsed < currentSequence.fadeTransitionDuration)
        {
            elapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Clamp01(elapsed / currentSequence.fadeTransitionDuration);
            yield return null;
        }

        dialogueCanvasGroup.alpha = 1;
    }

    private IEnumerator FadeOut()
    {
        if (dialogueCanvasGroup == null) yield break;

        dialogueCanvasGroup.alpha = 1;
        float elapsed = 0;

        while (elapsed < currentSequence.fadeTransitionDuration)
        {
            elapsed += Time.deltaTime;
            dialogueCanvasGroup.alpha = Mathf.Clamp01(1 - (elapsed / currentSequence.fadeTransitionDuration));
            yield return null;
        }

        dialogueCanvasGroup.alpha = 0;
    }

    public void SkipDialogue()
    {
        isPlaying = false;
        StopAllCoroutines();
        StartCoroutine(FadeOut());
    }
}