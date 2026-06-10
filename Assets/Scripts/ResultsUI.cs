using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class ResultsUI : MonoBehaviour
{
    public static UnityAction OnPlayerWin;

    [SerializeField] private TextMeshProUGUI timeTextBox;
    [SerializeField] private TextMeshProUGUI goldTextBox;
    [SerializeField] private TextMeshProUGUI enemiesTextBox;
    [SerializeField] private TextMeshProUGUI minesTextBox;

    private void Start()
    {
        InitializeResults();
    }

    private void InitializeResults()
    {
        timeTextBox.text = "Time: " + GameData.Instance.TimePassed;
        goldTextBox.text = "Gold Found: " + GameData.Instance.GoldFoundThisMatch + "/" + GameData.Instance.TotalGold;
        enemiesTextBox.text = "Enemies Defeated: " + GameData.Instance.EnemiesDefeatedThisMatch + "/" + GameData.Instance.TotalEnemies;
        minesTextBox.text = "Mines Found: " + GameData.Instance.MinesFoundThisMatch + "/" + GameData.Instance.TotalMines;
    }

    public void ExitResultsScreen()
    {
        Destroy(GameData.Instance.gameObject);

        if (MapManager.Instance != null)
        {
            if (MapManager.Instance.IsMapDone)
            {
                OnPlayerWin?.Invoke();

                if (ArtifactManager.Instance != null)
                    ArtifactManager.Instance.ClearAll();

                Destroy(MapManager.Instance.gameObject);
                Destroy(PlayerRunStats.Instance.gameObject);

                if (SceneTransitionManager.Instance != null)
                    SceneTransitionManager.Instance.LoadScene("WinScreen");
                else SceneManager.LoadScene("WinScreen");
                return;
            }
        }

        if (SceneTransitionManager.Instance != null)
            SceneTransitionManager.Instance.LoadScene("MapTesting");
        else SceneManager.LoadScene("MapTesting");
    }
}
