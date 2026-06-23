using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI goldText;

    private void Start()
    {
        InitializeHUD();
    }

    private void InitializeHUD()
    {
        if (PlayerRunStats.Instance != null)
        {
            PlayerRunStats.Instance.OnHealthChanged += UpdateHPUI;
            UpdateHPUI(PlayerRunStats.Instance.CurrentHP, PlayerRunStats.Instance.MaxHp);

            PlayerRunStats.Instance.OnRunGoldChanged += UpdateGoldUI;
            UpdateGoldUI(PlayerRunStats.Instance.GoldCollectedThisRun);
        }
    }

    private void OnDisable()
    {
        if (PlayerRunStats.HasInstance)
        {
            PlayerRunStats.Instance.OnHealthChanged -= UpdateHPUI;
            PlayerRunStats.Instance.OnRunGoldChanged -= UpdateGoldUI;
        }
    }

    private void UpdateGoldUI(int goldFound)
    {
        goldText.text = $"{goldFound}";
    }

    private void UpdateHPUI(int currentHP, int maxHP)
    {
        hpText.text = $"{currentHP}/{maxHP}";
    }
}
