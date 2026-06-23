using TMPro;
using UnityEngine;

public class GoldLostDisplayUI : MonoBehaviour
{
    [Header("Text Fields")]
    [SerializeField] private TextMeshProUGUI goldLostDisplay;

    private void Start() => ProcessAndDisplayLossText();

    private void ProcessAndDisplayLossText()
    {
        if (!PlayerRunStats.HasInstance) return;

        int totalRunGold = PlayerRunStats.Instance.GoldCollectedThisRun;
        float penaltyPercent = PlayerRunStats.Instance.LossPenaltyPercentage;

        int goldLostValue = Mathf.FloorToInt(totalRunGold * penaltyPercent);
        int goldSurvivingValue = Mathf.Max(0, totalRunGold - goldLostValue);

        if (goldLostDisplay) goldLostDisplay.text = $"Made it out with {goldSurvivingValue} gold ({goldLostValue} gold lost)";

        PlayerRunStats.Instance.CommitRunGoldOnLoss(goldSurvivingValue);
    }
}
