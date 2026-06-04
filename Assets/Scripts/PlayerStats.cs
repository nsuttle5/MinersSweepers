using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerStats : MonoBehaviour
{
    private static PlayerStats _instance;
    public static PlayerStats Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerStats>();
                if (_instance == null)
                {
                    GameObject go = new("PlayerStats");
                    _instance = go.AddComponent<PlayerStats>();
                }
            }
            return _instance;
        }
    }
    public static bool HasInstance => _instance != null;

    [Header("Health")]
    [SerializeField] private int maxHP = 100;

    private int currentHP;
    private int currentGold;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public int CurrentGold => currentGold;

    public UnityAction<int, int> OnHealthChanged;
    public UnityAction<int> OnGoldChanged;
    public UnityAction OnPlayerDeath;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ResetHealth();
    }

    public void ModifyHealth(int amount)
    {
        currentHP = Mathf.Clamp(currentHP + amount, 0, maxHP);
        OnHealthChanged?.Invoke(currentHP, maxHP);

        if (currentHP <= 0)
        {
            OnPlayerDeath?.Invoke();

            if (SceneTransitionManager.Instance != null)
                SceneTransitionManager.Instance.LoadScene("LoseScreen");
            else SceneManager.LoadScene("LoseScreen");
        }
    }

    public void ModifyGold(int amount)
    {
        currentGold = Mathf.Max(currentGold + amount, 0);
        OnGoldChanged?.Invoke(currentGold);
    }

    public void ResetHealth()
    {
        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }

    public void ResetGold()
    {
        currentGold = 0;
        OnGoldChanged?.Invoke(currentGold);
    }
}
