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
        }

        if (PlayerProfileManager.Instance != null)
        {
            PlayerProfileManager.Instance.OnGlobalGoldChanged += UpdateGoldUI;
            UpdateGoldUI(PlayerProfileManager.Instance.TotalGold);
        }
    }

    private void OnDisable()
    {
        if (PlayerProfileManager.HasInstance)
            PlayerProfileManager.Instance.OnGlobalGoldChanged -= UpdateGoldUI;
        if (PlayerRunStats.HasInstance)
            PlayerRunStats.Instance.OnHealthChanged -= UpdateHPUI;
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
