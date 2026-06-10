using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class GameData : MonoBehaviour
{
    #region Instance
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
    public static bool HasInstance => _instance != null;
    #endregion

    private float timePassed;

    public int MinesFoundThisMatch { get; private set; }
    public int GoldFoundThisMatch { get; private set; }
    public int EnemiesDefeatedThisMatch { get; private set; }
    public int TotalMines { get; private set; }
    public int TotalGold { get; private set; }
    public int TotalEnemies { get; private set; }
    public string TimePassed => GetTime();
    public bool GameStarted { get; private set; }

    public UnityAction<int> OnGoldChanged;

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

    private void Update()
    {
        if (!GameStarted) return;

        timePassed += Time.deltaTime;
    }

    public void SetMineData(int totalGold, int totalMines, int totalEnemies)
    {
        GoldFoundThisMatch = 0;
        MinesFoundThisMatch = 0;
        EnemiesDefeatedThisMatch = 0;

        TotalGold = totalGold;
        TotalMines = totalMines;
        TotalEnemies = totalEnemies;

        timePassed = 0;
        StopGame();
    }

    private string GetTime()
    {
        int minutes = Mathf.FloorToInt(timePassed / 60);
        int remainingSeconds = Mathf.FloorToInt(timePassed % 60);
        return string.Format("{0}:{1:00}", minutes, remainingSeconds);
    }

    public void CollectGold(int amount) //GoldEvent call
    {
        GoldEvent goldEvent = new GoldEvent(amount);
        GameEvents.OnGoldCollected?.Invoke(goldEvent);
        int finalAmount = goldEvent.FinalAmount;

        GoldFoundThisMatch += finalAmount;
        OnGoldChanged?.Invoke(GoldFoundThisMatch);
        PlayerProfileManager.Instance.AddGoldToWallet(finalAmount);
    }

    public void DefeatedEnemy() => EnemiesDefeatedThisMatch++;
    public void FoundMine() => MinesFoundThisMatch++;
    public void StartGame()
    {
        GameStarted = true;
        GameEvents.OnRunStart?.Invoke(); //GameStart event call
    }
    public void StopGame()
    {
        GameStarted = false;
        GameEvents.OnRunEnd?.Invoke(); //GameEnd event call
        GameEvents.OnFloorComplete?.Invoke(); //FloorComplete event call
    }

    public void SpendGold(int amount)
    {
        GameEvents.OnGoldSpent?.Invoke(amount); //GoldSpent event call
        PlayerProfileManager.Instance.TrySpendGold(amount);
    }
}
