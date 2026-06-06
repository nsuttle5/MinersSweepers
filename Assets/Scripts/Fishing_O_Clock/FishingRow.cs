using UnityEngine;
using System.Collections.Generic;

public class FishingRow : MonoBehaviour
{
    public List<FishingCell> cells = new();
    public bool hasPassedTop = false;

    public void Setup(GameObject cellPrefab, int columnCount, float cellSize, FishingConfigSO config, float elapsedTime)
    {
        foreach (var cell in cells)
            Destroy(cell.gameObject);
        cells.Clear();

        float totalWidth = (columnCount - 1) * cellSize;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < columnCount; i++)
        {
            GameObject cellGO = Instantiate(cellPrefab, transform);
            cellGO.transform.localPosition = new Vector3(startX + i * cellSize, 0f, 0f);
            FishingCell cell = cellGO.GetComponent<FishingCell>();
            cell.Setup(DetermineType(config, elapsedTime));
            cells.Add(cell);
        }
    }

    private FishingCellType DetermineType(FishingConfigSO config, float elapsedTime)
    {
        FishingDifficultyTier tier = config.difficultyTiers[0];
        for (int i = config.difficultyTiers.Count - 1; i >= 0; i--)
        {
            if (elapsedTime >= config.difficultyTiers[i].timeThreshold)
            {
                tier = config.difficultyTiers[i];
                break;
            }
        }

        float roll = Random.value;
        if (roll < tier.bombChance) return FishingCellType.Bomb;
        if (roll < tier.bombChance + tier.fishChance) return FishingCellType.Fish;
        return FishingCellType.Empty;
    }

    public bool HasUnrevealedBomb()
    {
        foreach (var cell in cells)
            if (cell.cellType == FishingCellType.Bomb && !cell.isRevealed && !cell.isDestroyed)
                return true;
        return false;
    }

    public void MarkAllDestroyed()
    {
        foreach (var cell in cells)
            cell.isDestroyed = true;
    }
}