using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleTextBox;
    [SerializeField] private TextMeshProUGUI bodyTextBox;
    [SerializeField] private Image artworkImage;
    [SerializeField] private Transform choiceButtonContainer;

    [Header("Prefabs")]
    [SerializeField] private InteractionButtonUI buttonPrefab;

    [Header("Testing")]
    [SerializeField] private List<InteractionDataSO> interactions;

    private void Start()
    {
        RollInteraction();
    }

    [ContextMenu("RollInteraction")]
    private void RollInteraction()
    {
        List<InteractionDataSO> unseenInteractions = new();

        foreach (InteractionDataSO interaction in interactions)
        {
            if (!PlayerRunStats.Instance.IsInteractionSeen(interaction.interactionID))
                unseenInteractions.Add(interaction);
        }

        if (unseenInteractions.Count == 0) unseenInteractions = new(interactions);

        InteractionDataSO selectedInteraction = unseenInteractions[Random.Range(0, unseenInteractions.Count)];
        PlayerRunStats.Instance.MarkInteractionAsSeen(selectedInteraction.interactionID);
        DisplayInteraction(selectedInteraction);
    }

    public void DisplayInteraction(InteractionDataSO data)
    {
        titleTextBox.text = data.interactionTitle;
        bodyTextBox.text = data.initialDescription;
        artworkImage.sprite = data.interactionArtwork;

        ClearButtons();

        foreach (InteractionChoice choice in data.choices)
        {
            InteractionButtonUI btn = Instantiate(buttonPrefab, choiceButtonContainer);
            bool canChoose = ValidateRequirement(choice);

            btn.Setup(choice, canChoose, () => OnChoiceSelected(choice));
        }
    }

    private void OnChoiceSelected(InteractionChoice choice)
    {
        ChoiceOutcome winningOutcome = PickRandomOutcome(choice.potentialOutcomes);

        if (winningOutcome != null)
        {
            foreach (AppliedEffect appliedEffect in winningOutcome.effects)
                ExecuteEffect(appliedEffect.effectType, appliedEffect.value);
            bodyTextBox.text = winningOutcome.outcomeText;
        }

        SetupExitButton();
    }

    private ChoiceOutcome PickRandomOutcome(List<ChoiceOutcome> outcomes)
    {
        if (outcomes == null) return null;
        if (outcomes.Count == 0) return null;
        if (outcomes.Count == 1) return outcomes[0];

        int totalWeight = 0;
        foreach (ChoiceOutcome outcome in outcomes) totalWeight += outcome.weight;

        int roll = Random.Range(0, totalWeight);
        int currentWeightSum = 0;

        foreach (ChoiceOutcome outcome in outcomes)
        {
            currentWeightSum += outcome.weight;
            if (roll < currentWeightSum) return outcome;
        }

        return outcomes[^1];
    }

    private void SetupExitButton()
    {
        ClearButtons();
        InteractionButtonUI btn = Instantiate(buttonPrefab, choiceButtonContainer);
        btn.SetupSimple("Leave", () => ReturnToMap());
    }

    private void ClearButtons()
    {
        foreach (Transform child in choiceButtonContainer)
        {
            Destroy(child.gameObject);
        }
    }

    private void ReturnToMap()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("MapTesting");
        else
            SceneManager.LoadScene("MapTesting");
    }

    public bool ValidateRequirement(InteractionChoice choice)
    {
        return choice.type switch
        {
            InteractionRequirement.HPAmount => PlayerRunStats.Instance.CurrentHP >= choice.requirementValue,
            InteractionRequirement.GoldAmount => PlayerProfileManager.Instance.TotalGold >= choice.requirementValue,
            InteractionRequirement.None => true,
            _ => true,
        };
    }

    public void ExecuteEffect(InteractionEffect effect, int value)
    {
        switch (effect)
        {
            case InteractionEffect.ChangeGold:
                PlayerProfileManager.Instance.AddGoldToWallet(value);
                break;
            case InteractionEffect.ChangeHP:
                PlayerRunStats.Instance.ModifyHealth(value);
                break;
            case InteractionEffect.None:
            default:
                break;
        }
    }
}
