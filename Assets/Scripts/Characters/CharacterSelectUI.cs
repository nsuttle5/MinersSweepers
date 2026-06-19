using UnityEngine;
using System.Collections.Generic;

public class CharacterSelectUI : MonoBehaviour
{
    [SerializeField] private List<CharacterSelectButton> characterButtons;
    [SerializeField] private CharacterSO defaultCharacter;

    private void OnEnable()
    {
        CharacterSelectButton.OnCharacterButtonClicked += HandleCharacterSelected;
    }

    private void OnDisable()
    {
        CharacterSelectButton.OnCharacterButtonClicked -= HandleCharacterSelected;
    }

    private void Start()
    {
        CharacterSO toSelect = CharacterManager.Instance.HasSelection ? CharacterManager.Instance.SelectedCharacter : defaultCharacter;

        HandleCharacterSelected(toSelect);
    }

    private void HandleCharacterSelected(CharacterSO character)
    {
        CharacterManager.Instance.SelectCharacter(character);

        foreach (var button in characterButtons)
        {
            button.SetSelected(button.Character == character);
        }
    }
}
