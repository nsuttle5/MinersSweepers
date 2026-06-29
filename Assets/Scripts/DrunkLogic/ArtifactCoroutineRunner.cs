using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ArtifactCoroutineRunner : MonoBehaviour
{
    public static ArtifactCoroutineRunner Instance { get; private set; }

    private HashSet<Coroutine> _tracked = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public Coroutine StartTracked(IEnumerator routine)
    {
        var c = StartCoroutine(routine);
        _tracked.Add(c);
        return c;
    }

    public void StopTracked(Coroutine c)
    {
        if (c == null) return;
        _tracked.Remove(c);
        StopCoroutine(c);
    }
}