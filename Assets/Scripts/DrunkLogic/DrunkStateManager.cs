using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class DrunkStateManager : MonoBehaviour
{
    public static DrunkStateManager Instance { get; private set; }

    public bool IsDrunk { get; private set; } = false;
    public static UnityAction OnDrunkStart;
    public static UnityAction OnSobered;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartDrunk()
    {
        IsDrunk = true;
        OnDrunkStart?.Invoke();
    }

    public void Sober()
    {
        IsDrunk = false;
        OnSobered?.Invoke();
    }
}