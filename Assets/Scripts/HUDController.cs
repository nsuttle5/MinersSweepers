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
        if (GameData.Instance != null)
        {
            UpdateGoldUI(GameData.Instance.GoldFound);
            GameData.Instance.OnGoldChanged += UpdateGoldUI;
        }
        if (PlayerStats.Instance != null)
        {
            UpdateHPUI(PlayerStats.Instance.CurrentHP, PlayerStats.Instance.MaxHP);
            PlayerStats.Instance.OnHealthChanged += UpdateHPUI;
        }
    }

    private void OnDisable()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnGoldChanged -= UpdateGoldUI;
        if (PlayerStats.Instance != null)
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
