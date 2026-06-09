using TMPro;
using UnityEngine;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerTextBox;

    private void Update()
    {
        if (GameData.HasInstance)
        {
            timerTextBox.text = "Timer: " + GameData.Instance.TimePassed;
        }
    }
}
