using UnityEngine;
using System.Collections.Generic;

public class ArtifactManager : MonoBehaviour
{
    public static ArtifactManager Instance { get; private set; }

    private List<ArtifactSO> _artifacts = new();
    private List<ConsumableSO> _consumables = new();

    public IReadOnlyList<ArtifactSO> Artifacts => _artifacts;
    public IReadOnlyList<ConsumableSO> Consumables => _consumables;

    private bool _loadoutAppliedThisRun = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameEvents.OnRunStart += HandleRunStart;
        GameEvents.OnRunEnd += HandleRunEnd;
    }

    private void OnDisable()
    {
        GameEvents.OnRunStart -= HandleRunStart;
        GameEvents.OnRunEnd -= HandleRunEnd;
    }

    public void AddArtifact(ArtifactSO artifact)
    {
        if (_artifacts.Contains(artifact)) return;
        _artifacts.Add(artifact);
        artifact.OnObtain();
        GameEvents.OnArtifactObtained?.Invoke(artifact);
    }

    public void RemoveArtifact(ArtifactSO artifact)
    {
        if (!_artifacts.Contains(artifact)) return;
        artifact.OnRemove();
        _artifacts.Remove(artifact);
    }

    public void AddConsumable(ConsumableSO consumable)
    {
        _consumables.Add(consumable);
    }

    public void RemoveConsumable(ConsumableSO consumable)
    {
        _consumables.Remove(consumable);
    }

    public void UseConsumable(ConsumableSO consumable)
    {
        if (!_consumables.Contains(consumable)) return;
        consumable.OnUse();
    }

    public bool HasArtifact<T>() where T : ArtifactSO
    {
        foreach (var a in _artifacts)
            if (a is T) return true;
        return false;
    }

    private void HandleRunStart()
    {
        foreach (var artifact in _artifacts)
            artifact.OnRunStart();
    }

    private void HandleRunEnd()
    {
        foreach (var artifact in _artifacts)
            artifact.OnRunEnd();
    }

    public void ClearAll()
    {
        foreach (var artifact in _artifacts)
            artifact.OnRemove();
        _artifacts.Clear();
        _consumables.Clear();
        _loadoutAppliedThisRun = false;
    }

    public void ApplyCharacterLoadout(CharacterSO character)
    {
        if (character == null) return;

        foreach (var artifact in character.characterPassives)
        {
            if (!_artifacts.Contains(artifact))
            {
                _artifacts.Add(artifact);
                artifact.OnObtain();
            }
        }

        foreach (var artifact in character.startingArtifacts)
        {
            AddArtifact(artifact);
        }

        foreach (var consumable in character.startingConsumables)
        {
            AddConsumable(consumable);
        }

        _loadoutAppliedThisRun = true;
    }
}