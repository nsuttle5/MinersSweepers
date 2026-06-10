using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class PlayerRunStats : MonoBehaviour
{
    #region Instance
    private static PlayerRunStats _instance;
    public static PlayerRunStats Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<PlayerRunStats>();
                if (_instance == null)
                {
                    GameObject go = new("RunStats");
                    _instance = go.AddComponent<PlayerRunStats>();
                }
            }
            return _instance;
        }
    }
    public static bool HasInstance => _instance != null;
    #endregion

    [Header("Health")]
    [SerializeField] private int startingMaxHP = 100;

    public int CurrentHP { get; set; }
    public int MaxHp { get; private set; }

    public UnityAction<int, int> OnHealthChanged;
    public UnityAction OnPlayerDeath;

    private HashSet<string> seenInteractionIDs = new();

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        seenInteractionIDs = new();
        GenerateRunStats();
    }

    private void OnEnable()
    {
        TavernManager.OnStateChanged += Refresh;
    }

    private void OnDisable()
    {
        TavernManager.OnStateChanged -= Refresh;
    }

    private void GenerateRunStats()
    {
        int hpLevel = PlayerProfileManager.Instance.HpUpgradeLevel;
        MaxHp = startingMaxHP + ((hpLevel - 1) * 10);
        CurrentHP = MaxHp;
    }

    public void ModifyHealth(int amount)
    {
        if (amount < 0) //DamageEvent call
        {
            DamageEvent dmgEvent = new DamageEvent(-amount);
            GameEvents.OnDamageReceived?.Invoke(dmgEvent);
            amount = -dmgEvent.FinalDamage;
        }
        else if (amount > 0) GameEvents.OnPlayerHealed?.Invoke(amount); //Heal Event

        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHp);
        OnHealthChanged?.Invoke(CurrentHP, MaxHp);
        GameEvents.OnHealthChanged?.Invoke(CurrentHP, MaxHp); //Health Changed Event

        if (CurrentHP > 0 && CurrentHP <= MaxHp * 0.25f) GameEvents.OnLowHealth?.Invoke(); //Low Health Event

        if (CurrentHP == MaxHp) GameEvents.OnFullHealth?.Invoke(); //Full Health Event

        if (CurrentHP > 0) return;

        GameEvents.OnPlayerDeath?.Invoke(); //Player Death Event
        OnPlayerDeath?.Invoke();
        Destroy(gameObject);

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("LoseScreen");
        else SceneManager.LoadScene("LoseScreen");
    }

    private void Refresh()
    {
        int hpLevel = PlayerProfileManager.Instance.HpUpgradeLevel;
        MaxHp = startingMaxHP + ((hpLevel - 1) * 10);
        OnHealthChanged?.Invoke(CurrentHP, MaxHp);
    }

    public void MarkInteractionAsSeen(string id)
    {
        if (!string.IsNullOrEmpty(id)) seenInteractionIDs.Add(id);
    }

    public bool IsInteractionSeen(string id)
    {
        return seenInteractionIDs.Contains(id);
    }
}
