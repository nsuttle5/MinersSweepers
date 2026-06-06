using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TavernPowerup : MonoBehaviour
{
    [SerializeField] private TavernUpgradeType upgradeType;
    [SerializeField] private int cost = 10;
    [SerializeField] private int maxLevel = -1;

    [Header("UI Fields")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private TMP_Text levelText;

    private Button button;

    private void Awake()
    {
        TryGetComponent(out button);
    }

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(HandleClick);
        TavernManager.OnStateChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(HandleClick);
        TavernManager.OnStateChanged -= Refresh;
    }

    private void HandleClick()
    {
        if (TavernManager.TryPurchaseUpgrade(upgradeType, cost, maxLevel)) Refresh();
    }

    public void Refresh()
    {
        if (button == null) return;

        int currentLevel = TavernManager.GetUpgradeLevel(upgradeType);
        bool atMaxLevel = maxLevel >= 0 && currentLevel >= maxLevel;

        button.interactable = !atMaxLevel && (PlayerProfileManager.Instance.TotalGold >= cost);

        if (titleText != null)
        {
            titleText.text = upgradeType.ToString();
        }

        if (costText != null)
        {
            costText.text = atMaxLevel ? "MAX" : $"Cost: {cost}";
        }

        if (levelText != null)
        {
            levelText.text = $"Lv. {currentLevel}";
        }
    }
}
