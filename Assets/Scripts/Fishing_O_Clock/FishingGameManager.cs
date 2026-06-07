using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class FishingGameManager : MonoBehaviour
{
    public static FishingGameManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TextMeshProUGUI gameOverReasonText;
    [SerializeField] private TextMeshProUGUI fishCaughtText;
    [SerializeField] private TextMeshProUGUI timeAliveText;
    [SerializeField] private TextMeshProUGUI goldEarnedText;

    [Header("Rewards")]
    [SerializeField] private int goldPerFish = 10;
    [SerializeField] private string mapSceneName = "MapScene";

    private int _fishCaught;
    private int _goldEarned;
    private bool _gameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        gameOverPanel.SetActive(false);
        _fishCaught = 0;
        _goldEarned = 0;
        _gameOver = false;
        FishingBoardManager.Instance.StartGame();
    }

    public void OnFishCaught()
    {
        if (_gameOver) return;
        _fishCaught++;
        _goldEarned += goldPerFish;
        GameData.Instance.CollectGold(goldPerFish);
    }

    public void TriggerGameOver(GameOverReason reason)
    {
        if (_gameOver) return;
        _gameOver = true;

        FishingBoardManager.Instance.StopGame();

        float elapsed = FishingBoardManager.Instance.GetElapsedTime();

        if (gameOverReasonText) gameOverReasonText.text = reason == GameOverReason.BombClicked
            ? "Bomb hit" : "Bomb reached the top";
        if (fishCaughtText) fishCaughtText.text = $"Fish Caught: {_fishCaught}";
        if (timeAliveText) timeAliveText.text = $"Time: {elapsed:F1}s";
        if (goldEarnedText) goldEarnedText.text = $"Gold Earned: {_goldEarned}";

        gameOverPanel.SetActive(true);
    }

    public void OnExitPressed()
    {
        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene(mapSceneName);
        else
            SceneManager.LoadScene(mapSceneName);
    }
}