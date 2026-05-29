using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class LogbookUI : MonoBehaviour
{
    public GameObject logbookPanel;
    public Button logbookButton;
    public Transform entryGridParent;
    public GameObject entrySlotPrefab;

    public TMP_Text tutorialText;
    public GameObject detailGroup;
    public TMP_Text nameText, damageText, abilitiesText, descriptionText;
    public Image detailSpriteImg;

    private List<GameObject> entrySlots = new List<GameObject>();

    public Button closeButton;

    void Start()
    {
        logbookButton.onClick.AddListener(OpenLogbook);
        logbookPanel.SetActive(false);
        closeButton.onClick.AddListener(CloseLogbook);

        foreach (var so in LogbookManager.Instance.allEntries)
        {
            var slotGO = Instantiate(entrySlotPrefab, entryGridParent);
            var slot = slotGO.GetComponent<LogbookEntrySlot>();
            slot.SetEntry(so, LogbookManager.Instance.IsDiscovered(so));
            slot.OnEntryClicked = ShowEntryDetails;
            entrySlots.Add(slotGO);
        }
    }

    void Update()
    {
        if (logbookPanel.activeSelf && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseLogbook();
        }
    }

    public void OpenLogbook()
    {
        BoardManager.isLogbookOpen = true;
        logbookPanel.SetActive(true);
        tutorialText.gameObject.SetActive(true);
        detailGroup.SetActive(false);

        for (int i = 0; i < entrySlots.Count; i++)
        {
            var so = LogbookManager.Instance.allEntries[i];
            entrySlots[i].GetComponent<LogbookEntrySlot>()
                .SetEntry(so, LogbookManager.Instance.IsDiscovered(so));
        }
    }

    public void ShowEntryDetails(SpawnableSO so)
    {
        tutorialText.gameObject.SetActive(false);
        detailGroup.SetActive(true);
        nameText.text = so.displayName;
        damageText.text = so.damage.ToString();
        abilitiesText.text = so.abilityTooltip;
        descriptionText.text = so.description;
        detailSpriteImg.sprite = so.sprite;
    }

    public void CloseLogbook()
    {
        logbookPanel.SetActive(false);
        BoardManager.isLogbookOpen = false;
    }
}