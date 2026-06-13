using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BoardSidebarSlotUI : MonoBehaviour
{
    [SerializeField] private Image spawnableIcon;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Color discoveredColor = Color.white;
    [SerializeField] private Color undiscoveredColor = Color.black;

    public void Setup(BoardSidebarEntry entry)
    {
        if (entry.discovered)
        {
            spawnableIcon.sprite = entry.spawnable.sprite;
            spawnableIcon.color = discoveredColor;
            damageText.text = entry.spawnable.damage > 0 ? $"P{entry.spawnable.damage}" : "P0";
            damageText.gameObject.SetActive(true);
        }
        else
        {
            spawnableIcon.sprite = entry.spawnable.sprite;
            spawnableIcon.color = undiscoveredColor;
            damageText.text = "?";
            damageText.gameObject.SetActive(true);
        }

        string countDisplay = entry.count > 9 ? $"x{entry.count}" : $"x{entry.count}";
        if (!entry.discovered)
            countDisplay = "x?";

        countText.text = countDisplay;
    }
}