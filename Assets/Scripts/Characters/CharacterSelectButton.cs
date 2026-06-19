using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private CharacterSO character;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedIndicator;

    public static System.Action<CharacterSO> OnCharacterButtonClicked;

    private void Start()
    {
        button.onClick.AddListener(() => OnCharacterButtonClicked?.Invoke(character));
        SetSelected(false);
    }

    public CharacterSO Character => character;

    public void SetSelected(bool selected)
    {
        if (selectedIndicator) selectedIndicator.SetActive(selected);
    }
}
