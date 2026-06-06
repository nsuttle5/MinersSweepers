using UnityEngine;
using UnityEngine.Events;

public class PlayerProfileManager : MonoBehaviour
{
    #region Instance
    private static PlayerProfileManager _instance;
    public static PlayerProfileManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerProfileManager>();
                if (_instance == null)
                {
                    GameObject gameDataGO = new("ProfileManager");
                    _instance = gameDataGO.AddComponent<PlayerProfileManager>();
                }
            }
            return _instance;
        }
    }
    public static bool HasInstance => _instance != null;
    #endregion

    public int TotalGold { get; set; } = 0;
    public int HpUpgradeLevel { get; set; } = 1;
    public int LuckUpgradeLevel { get; set; } = 1;

    public UnityAction<int> OnGlobalGoldChanged;

    private void Awake()
    {
        if (_instance != null && _instance != this) { Destroy(gameObject); return; }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddGoldToWallet(int amount)
    {
        TotalGold = Mathf.Max(0, TotalGold + amount);
        OnGlobalGoldChanged?.Invoke(TotalGold);
    }

    public bool TrySpendGold(int amount)
    {
        if (TotalGold >= amount)
        {
            TotalGold = Mathf.Max(0, TotalGold - amount);
            OnGlobalGoldChanged?.Invoke(TotalGold);
            return true;
        }
        return false;
    }
}
