using UnityEngine;
using UnityEngine.UI;

public class DialogueTestManager : MonoBehaviour
{
    [SerializeField] private GameObject dialoguePrefab;
    [SerializeField] private DialogueSequenceSO testSequence;
    [SerializeField] private Button playButton;

    private DialogueUIController dialogueController;

    private void Start()
    {
        playButton.onClick.AddListener(PlayTestDialogue);
    }

    private void PlayTestDialogue()
    {
        if (dialogueController == null)
        {
            GameObject dialogueInstance = Instantiate(dialoguePrefab);
            dialogueController = dialogueInstance.GetComponent<DialogueUIController>();
        }

        dialogueController.StartDialogueSequence(testSequence);
    }
}