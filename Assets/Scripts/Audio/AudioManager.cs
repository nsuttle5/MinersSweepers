using UnityEngine;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Source Pool")]
    [SerializeField] private int poolSize = 16;
    [SerializeField] private AudioSource musicSource;

    [Header("Global SFX Settings")]
    [SerializeField] private float masterSFXVolume = 1f;
    [SerializeField] private float masterMusicVolume = 1f;

    [Header("Board Sounds")]
    [SerializeField] private AudioClip blockBreakClip;
    [SerializeField] private AudioClip emptyRevealClip;
    [SerializeField] private AudioClip boardGenerateClip;
    [SerializeField] private AudioClip exitRevealClip;
    [SerializeField] private AudioClip goldCollectClip;
    [SerializeField] private AudioClip cascadeRevealClip;

    [Header("Player Sounds")]
    [SerializeField] private AudioClip playerHurtClip;
    [SerializeField] private AudioClip playerAttackClip;
    [SerializeField] private AudioClip playerDeathClip;
    [SerializeField] private AudioClip playerHealClip;
    [SerializeField] private AudioClip playerLowHealthClip;

    [Header("Enemy Sounds")]
    [SerializeField] private AudioClip enemyRevealClip;
    [SerializeField] private AudioClip enemyDeathClip;

    [Header("Tile Sounds")]
    [SerializeField] private AudioClip havenRevealClip;
    [SerializeField] private AudioClip bloodAltarRevealClip;
    [SerializeField] private AudioClip flareRevealClip;
    [SerializeField] private AudioClip caveQuakeRevealClip;
    [SerializeField] private AudioClip caveTotemRevealClip;
    [SerializeField] private AudioClip emptyRevealTileClip;
    [SerializeField] private AudioClip voidRevealClip;

    [Header("UI / Shop Sounds")]
    [SerializeField] private AudioClip shopOpenClip;
    [SerializeField] private AudioClip shopCloseClip;
    [SerializeField] private AudioClip itemPurchaseClip;
    [SerializeField] private AudioClip rerollClip;
    [SerializeField] private AudioClip cannotAffordClip;

    [Header("Map Sounds")]
    [SerializeField] private AudioClip nodeSelectClip;
    [SerializeField] private AudioClip mapTransitionClip;

    [Header("Run Sounds")]
    [SerializeField] private AudioClip runStartClip;
    [SerializeField] private AudioClip runWinClip;
    [SerializeField] private AudioClip runLoseClip;
    [SerializeField] private AudioClip bossDefeatedClip;

    [Header("Music")]
    [SerializeField] private AudioClip mineMusic;
    [SerializeField] private AudioClip mapMusic;
    [SerializeField] private AudioClip shopMusic;
    [SerializeField] private AudioClip bossMusic;

    private List<AudioSource> _pool = new();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildPool();
    }

    private void OnEnable()
    {
        GameEvents.OnCellRevealed += OnCellRevealed;
        GameEvents.OnEmptyCellRevealed += OnEmptyCellRevealed;
        GameEvents.OnEnemyRevealed += OnEnemyRevealed;
        GameEvents.OnEnemyDefeated += OnEnemyDefeated;
        GameEvents.OnGoldCollected += OnGoldCollected;
        GameEvents.OnExitRevealed += OnExitRevealed;
        GameEvents.OnDamageReceived += OnDamageReceived;
        GameEvents.OnPlayerHealed += OnPlayerHealed;
        GameEvents.OnPlayerDeath += OnPlayerDeath;
        GameEvents.OnLowHealth += OnLowHealth;
        GameEvents.OnBossDefeated += OnBossDefeated;
        GameEvents.OnBoardGenerated += OnBoardGenerated;
        GameEvents.OnShopOpened += OnShopOpened;
        GameEvents.OnShopClosed += OnShopClosed;
        GameEvents.OnItemPurchased += OnItemPurchased;
        GameEvents.OnShopRerolled += OnShopRerolled;
        GameEvents.OnRunStart += OnRunStart;
        GameEvents.OnRunEnd += OnRunEnd;
        GameEvents.OnFloorComplete += OnFloorComplete;
    }

    private void OnDisable()
    {
        GameEvents.OnCellRevealed -= OnCellRevealed;
        GameEvents.OnEmptyCellRevealed -= OnEmptyCellRevealed;
        GameEvents.OnEnemyRevealed -= OnEnemyRevealed;
        GameEvents.OnEnemyDefeated -= OnEnemyDefeated;
        GameEvents.OnGoldCollected -= OnGoldCollected;
        GameEvents.OnExitRevealed -= OnExitRevealed;
        GameEvents.OnDamageReceived -= OnDamageReceived;
        GameEvents.OnPlayerHealed -= OnPlayerHealed;
        GameEvents.OnPlayerDeath -= OnPlayerDeath;
        GameEvents.OnLowHealth -= OnLowHealth;
        GameEvents.OnBossDefeated -= OnBossDefeated;
        GameEvents.OnBoardGenerated -= OnBoardGenerated;
        GameEvents.OnShopOpened -= OnShopOpened;
        GameEvents.OnShopClosed -= OnShopClosed;
        GameEvents.OnItemPurchased -= OnItemPurchased;
        GameEvents.OnShopRerolled -= OnShopRerolled;
        GameEvents.OnRunStart -= OnRunStart;
        GameEvents.OnRunEnd -= OnRunEnd;
        GameEvents.OnFloorComplete -= OnFloorComplete;
    }


    private void OnCellRevealed(CellView cell) => AudioManager.Instance?.PlayWithRandomPitch(blockBreakClip, 0.6f, 0.85f, 1.15f);
    private void OnEmptyCellRevealed(CellView c) => Play(emptyRevealClip, 0.5f);
    private void OnEnemyRevealed(CellView c) => Play(enemyRevealClip);
    private void OnEnemyDefeated(CellView c) => Play(enemyDeathClip);
    private void OnExitRevealed(CellView c) => Play(exitRevealClip);
    private void OnBoardGenerated() => Play(boardGenerateClip, 0.7f);
    private void OnCascadeReveal(int count) => Play(cascadeRevealClip, 0.8f);
    private void OnBossDefeated(CellView c) => Play(bossDefeatedClip);


    private void OnGoldCollected(GoldEvent e) => Play(goldCollectClip);


    private void OnDamageReceived(DamageEvent e)
    {
        if (e.FinalDamage > 0) Play(playerHurtClip);
    }

    private void OnPlayerHealed(int amount) => Play(playerHealClip);
    private void OnPlayerDeath() => Play(playerDeathClip);
    private void OnLowHealth() => Play(playerLowHealthClip, 0.6f);


    private void OnBoardTileRevealed(CellView cell, BoardTileSO tile)
    {
        AudioClip clip = tile switch
        {
            HavenTileSO _ => havenRevealClip,
            BloodAltarTileSO _ => bloodAltarRevealClip,
            FlareTileSO _ => flareRevealClip,
            CaveQuakeTileSO _ => caveQuakeRevealClip,
            CaveTotemTileSO _ => caveTotemRevealClip,
            EmptyRevealTileSO _ => emptyRevealTileClip,
            VoidTile _ => voidRevealClip,
            _ => blockBreakClip
        };

        Play(clip);
    }


    private void OnShopOpened() => Play(shopOpenClip);
    private void OnShopClosed() => Play(shopCloseClip);
    private void OnItemPurchased(ShopSlotData s) => Play(itemPurchaseClip);
    private void OnShopRerolled(int cost) => Play(rerollClip);


    private void OnRunStart() => PlayMusic(mineMusic);
    private void OnRunEnd() { }
    private void OnFloorComplete() => Play(runWinClip, 0.5f);


    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null) return;
        if (musicSource.clip == clip && musicSource.isPlaying) return;
        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.volume = masterMusicVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource?.Stop();

    public void FadeOutMusic(float duration) => StartCoroutine(FadeMusicOut(duration));

    private System.Collections.IEnumerator FadeMusicOut(float duration)
    {
        if (musicSource == null) yield break;
        float start = musicSource.volume;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(start, 0f, elapsed / duration);
            yield return null;
        }
        musicSource.Stop();
        musicSource.volume = masterMusicVolume;
    }


    public void Play(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (clip == null) return;
        AudioSource source = GetFreeSource();
        if (source == null) return;
        source.clip = clip;
        source.volume = volume * masterSFXVolume;
        source.pitch = pitch;
        source.Play();
    }

    public void PlayWithRandomPitch(AudioClip clip, float volume = 1f,
        float minPitch = 0.9f, float maxPitch = 1.1f)
    {
        Play(clip, volume, Random.Range(minPitch, maxPitch));
    }

    private AudioSource GetFreeSource()
    {
        foreach (var src in _pool)
            if (!src.isPlaying) return src;

        AudioSource quietest = _pool[0];
        foreach (var src in _pool)
            if (src.volume < quietest.volume) quietest = src;
        return quietest;
    }

    private void BuildPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFXSource_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _pool.Add(src);
        }
    }
}