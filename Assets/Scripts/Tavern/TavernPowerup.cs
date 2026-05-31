using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class TavernPowerup : MonoBehaviour
{
    [SerializeField] private TavernManager tavernManager;
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
        button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);

        if (tavernManager == null)
        {
            tavernManager = TavernManager.Instance;
        }
    }

    private void OnEnable()
    {
        if (tavernManager == null)
        {
            tavernManager = TavernManager.Instance;
        }

        if (tavernManager != null)
        {
            tavernManager.OnStateChanged += Refresh;
        }

        Refresh();
    }

    private void OnDisable()
    {
        if (tavernManager != null)
        {
            tavernManager.OnStateChanged -= Refresh;
        }

        if (button != null)
        {
            button.onClick.RemoveListener(HandleClick);
        }
    }

    private void HandleClick()
    {
        if (tavernManager == null)
        {
            return;
        }

        if (tavernManager.TryPurchaseUpgrade(upgradeType, cost, maxLevel))
        {
            Refresh();
        }
    }

    public void Refresh()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (tavernManager == null)
        {
            tavernManager = TavernManager.Instance;
        }

        if (tavernManager == null)
        {
            button.interactable = false;
            return;
        }

        int currentLevel = tavernManager.GetUpgradeLevel(upgradeType);
        bool atMaxLevel = maxLevel >= 0 && currentLevel >= maxLevel;

        button.interactable = !atMaxLevel && tavernManager.CanAfford(cost);

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
