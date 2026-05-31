using UnityEngine;

//Calling all the custom class abilities
public abstract class SpawnableAbilitiesSO : ScriptableObject
{
    public abstract void OnReveal(CellView cell, BoardManager board);
}
