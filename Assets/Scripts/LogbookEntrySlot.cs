using UnityEngine;
using UnityEngine.UI;

public class LogbookEntrySlot : MonoBehaviour
{
    public Image iconImage;
    public Button entryButton;

    private SpawnableSO entrySO;
    public System.Action<SpawnableSO> OnEntryClicked;

    public void SetEntry(SpawnableSO so, bool discovered)
    {
        entrySO = so;
        iconImage.sprite = so.sprite;

        iconImage.color = discovered ? Color.white : Color.black;

        entryButton.interactable = discovered;
        entryButton.onClick.RemoveAllListeners();
        if (discovered)
            entryButton.onClick.AddListener(() => OnEntryClicked?.Invoke(entrySO));
    }
}