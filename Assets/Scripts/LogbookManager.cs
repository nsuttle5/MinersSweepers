using System.Collections.Generic;
using UnityEngine;

public class LogbookManager : MonoBehaviour
{
    public List<SpawnableSO> allEntries;

    // Discovered spawnables this game/session
    private HashSet<SpawnableSO> discovered = new HashSet<SpawnableSO>();

    public static LogbookManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        // allEntries = new List<SpawnableSO>(Resources.LoadAll<SpawnableSO>("Spawnables"));
        //^^ could auto load from resources folder if we want
    }

    public bool IsDiscovered(SpawnableSO so) => discovered.Contains(so);

    public void Discover(SpawnableSO so)
    {
        if (!discovered.Contains(so))
            discovered.Add(so);
    }
}