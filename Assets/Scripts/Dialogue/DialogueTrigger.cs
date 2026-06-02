using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    [SerializeField] private DialogueSequenceSO dialogueSequence;
    [SerializeField] private DialogueUIController dialogueController;

    private void Awake()
    {
        if (dialogueController == null)
        {
            dialogueController = FindAnyObjectByType<DialogueUIController>();
        }
    }

    public void PlayDialogue()
    {
        if (dialogueSequence == null)
        {
            Debug.LogError("No dialogue sequence assigned");
            return;
        }

        if (dialogueController == null)
        {
            Debug.LogError("No dialogue controller found");
            return;
        }

        dialogueController.StartDialogueSequence(dialogueSequence);
    }
}