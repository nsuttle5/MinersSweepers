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
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnGoldChanged += UpdateGoldUI;
            PlayerStats.Instance.OnHealthChanged += UpdateHPUI;
            UpdateGoldUI(PlayerStats.Instance.CurrentGold);
            UpdateHPUI(PlayerStats.Instance.CurrentHP, PlayerStats.Instance.MaxHP);
        }
    }

    private void OnDisable()
    {
        if (PlayerStats.HasInstance)
            PlayerStats.Instance.OnGoldChanged -= UpdateGoldUI;
        if (PlayerStats.HasInstance)
            PlayerStats.Instance.OnHealthChanged -= UpdateHPUI;
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
