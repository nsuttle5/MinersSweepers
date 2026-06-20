using UnityEngine;

[CreateAssetMenu(fileName = "HavenTile", menuName = "BoardTiles/HavenTile")]
public class HavenTileSO : BoardTileSO
{
    [Range(0f, 1f)] public float rewardChance = 0.5f;
    public int goldRewardMin = 5;
    public int goldRewardMax = 15;

    public override void OnBoardSpawn(CellView cell, BoardManager board) { }

    public override void OnReveal(CellView cell, BoardManager board)
    {
        if (Random.value <= rewardChance)
        {
            int gold = Random.Range(goldRewardMin, goldRewardMax + 1);
            GameData.Instance.CollectGold(gold);
        }
    }
}