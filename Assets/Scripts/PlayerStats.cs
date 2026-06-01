using UnityEngine;
using UnityEngine.Events;

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

    [Header("Health")]
    [SerializeField] private int maxHP = 100;
    private int currentHP;

    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;

    public UnityEvent<int, int> OnHealthChanged;
    public UnityEvent OnPlayerDeath;

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

        if (currentHP <= 0) OnPlayerDeath?.Invoke();
    }

    public void ResetHealth()
    {
        currentHP = maxHP;
        OnHealthChanged?.Invoke(currentHP, maxHP);
    }
}
