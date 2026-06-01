using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class InteractionButtonUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textBox;

    private Button button;

    private void Awake()
    {
        TryGetComponent(out button);
    }

    public void Setup(InteractionChoice choice, bool isInteractable, UnityAction onClickCallback)
    {
        textBox.text = choice.buttonText;
        button.interactable = isInteractable;

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke());
    }

    public void SetupSimple(string text, UnityAction onClickCallback)
    {
        textBox.text = text;
        button.interactable = true;
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => onClickCallback?.Invoke());
    }
}
