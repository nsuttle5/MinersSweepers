using TMPro;
using UnityEngine;

public class ResultsUI : MonoBehaviour
{
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
        goldTextBox.text = "Gold Found: " + GameData.Instance.GoldFound + "/" + GameData.Instance.TotalGold;
        enemiesTextBox.text = "Enemies Defeated: " + GameData.Instance.EnemiesDefeated + "/" + GameData.Instance.TotalEnemies;
        minesTextBox.text = "Mines Found: " + GameData.Instance.MinesFound + "/" + GameData.Instance.TotalMines;
    }
}
