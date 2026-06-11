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

    private List<ArtifactSO> artifacts;
    public IReadOnlyList<ArtifactSO> Artifacts => artifacts;

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
        artifacts = new();
        seenInteractionIDs = new();
        GenerateRunStats();
    }

    private void OnEnable()
    {
        TavernManager.OnStateChanged += Refresh;
        GameEvents.OnRunStart += HandleRunStart;
        GameEvents.OnRunEnd += HandleRunEnd;
    }

    private void OnDisable()
    {
        TavernManager.OnStateChanged -= Refresh;
        GameEvents.OnRunStart -= HandleRunStart;
        GameEvents.OnRunEnd -= HandleRunEnd;
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
            DamageEvent dmgEvent = new(-amount);
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

        foreach (ArtifactSO a in artifacts) a.OnRemove();
        artifacts.Clear();

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

    public void AddArtifact(ArtifactSO artifact)
    {
        if (artifacts.Contains(artifact)) return;
        artifacts.Add(artifact);
        artifact.OnObtain();
        GameEvents.OnArtifactObtained?.Invoke(artifact);
    }

    public void RemoveArtifact(ArtifactSO artifact)
    {
        if (!artifacts.Contains(artifact)) return;
        artifact.OnRemove();
        artifacts.Remove(artifact);
    }

    public bool HasArtifact<T>() where T : ArtifactSO
    {
        foreach (ArtifactSO a in artifacts)
            if (a is T) return true;
        return false;
    }

    private void HandleRunStart()
    {
        foreach (ArtifactSO a in artifacts) a.OnRunStart();
    }

    private void HandleRunEnd()
    {
        foreach (ArtifactSO a in artifacts) a.OnRunEnd();
    }
}
