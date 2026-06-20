using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BoardTileSpawnerArtifact", menuName = "Items/Artifacts/BoardTileSpawner")]
public class BoardTileSpawnerArtifact : ArtifactSO
{
    [System.Serializable]
    public class TileSpawnEntry
    {
        public BoardTileSO tileType;
        public int quantity = 1;
        public bool requiresEnemyUnderneath;
        public bool requiresEmptyUnderneath;
        [Range(0f, 1f)] public float spawnChance = 1f;
    }

    public List<TileSpawnEntry> tilesToSpawn;

    protected override void Subscribe()
    {
        GameEvents.OnBoardGenerated += OnBoardGenerated;
    }

    protected override void Unsubscribe()
    {
        GameEvents.OnBoardGenerated -= OnBoardGenerated;
    }

    private void OnBoardGenerated()
    {
        if (BoardManager.Instance == null) return;

        foreach (var entry in tilesToSpawn)
        {
            if (entry.tileType == null) continue;

            for (int i = 0; i < entry.quantity; i++)
            {
                if (Random.value > entry.spawnChance) continue;

                BoardManager.Instance.TryPlaceBoardTile(
                    entry.tileType,
                    entry.requiresEnemyUnderneath,
                    entry.requiresEmptyUnderneath);
            }
        }
    }
}