using UnityEngine;

public class BoardSidebarEntry
{
    public SpawnableSO spawnable;
    public int count;
    public bool discovered;

    public BoardSidebarEntry(SpawnableSO spawnable, int count, bool discovered)
    {
        this.spawnable = spawnable;
        this.count = count;
        this.discovered = discovered;
    }
}