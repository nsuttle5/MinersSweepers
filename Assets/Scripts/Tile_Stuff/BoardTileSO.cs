using UnityEngine;

public abstract class BoardTileSO : ScriptableObject
{
    [Header("Tile Info")]
    public string tileName;
    public Sprite tileSprite;
    [TextArea] public string description;

    public abstract void OnReveal(CellView cell, BoardManager board);

    public virtual void OnBoardSpawn(CellView cell, BoardManager board) { }
}
