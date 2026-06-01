using System.Collections.Generic;
using UnityEngine;

public class GameData : MonoBehaviour
{
    private static GameData _instance;
    public static GameData Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<GameData>();
                if (_instance == null)
                {
                    GameObject gameDataGO = new("GameData");
                    _instance = gameDataGO.AddComponent<GameData>();
                }
            }
            return _instance;
        }
    }

    public int MinesFound { get; set; }
    public int GoldFound { get; set; }
    public int EnemiesDefeated { get; set; }
    public int TotalMines { get; set; }
    public int TotalGold { get; set; }
    public int TotalEnemies { get; set; }
    public string TimePassed => GetTime();
    public bool GameStarted { get; set; }

    private float timePassed;
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
    }

    private void Update()
    {
        if (!GameStarted) return;

        timePassed += Time.deltaTime;
    }

    public void ResetData()
    {
        MinesFound = 0;
        EnemiesDefeated = 0;
        GoldFound = 0;
        TotalMines = 0;
        TotalEnemies = 0;
        TotalGold = 0;
        timePassed = 0;
        GameStarted = false;

        if (PlayerStats.Instance != null) PlayerStats.Instance.ResetHealth();

        seenInteractionIDs.Clear();
    }

    private string GetTime()
    {
        int minutes = Mathf.FloorToInt(timePassed / 60);
        int remainingSeconds = Mathf.FloorToInt(timePassed % 60);
        return string.Format("{0}:{1:00}", minutes, remainingSeconds);
    }

    public void MarkInteractionAsSeen(string id)
    {
        if (!string.IsNullOrEmpty(id)) seenInteractionIDs.Add(id);
    }

    public bool IsInteractionSeen(string id)
    {
        return seenInteractionIDs.Contains(id);
    }

    public bool ValidateRequirement(InteractionChoice choice)
    {
        return choice.type switch
        {
            InteractionRequirement.HPAmount => PlayerStats.Instance.CurrentHP >= choice.requirementValue,
            InteractionRequirement.GoldAmount => GoldFound >= choice.requirementValue,
            InteractionRequirement.None => true,
            _ => true,
        };
    }

    public void ExecuteEffect(InteractionEffect effect, int value)
    {
        switch (effect)
        {
            case InteractionEffect.ChangeGold:
                GoldFound = Mathf.Max(0, GoldFound + value);
                break;
            case InteractionEffect.ChangeHP:
                PlayerStats.Instance.ModifyHealth(value);
                break;
            case InteractionEffect.None:
            default:
            break;
        }
    }
}
