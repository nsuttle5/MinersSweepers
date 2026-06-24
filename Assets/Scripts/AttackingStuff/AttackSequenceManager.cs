using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class AttackSequenceManager : MonoBehaviour
{
    public static AttackSequenceManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform playerUITransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform animationRoot;
    [SerializeField] private Camera mainCamera;

    [Header("Player Sprite")]
    [SerializeField] private UnityEngine.UI.Image playerImage;
    [SerializeField] private Sprite playerIdleSprite;
    [SerializeField] private Sprite playerHitSprite;
    [SerializeField] private Sprite[] playerAttackFrames;
    [SerializeField] private float playerAttackFrameDuration = 0.1f;
    [SerializeField] private float playerHitDuration = 0.6f;
    [SerializeField] private float playerHitKnockback = 15f;
    [SerializeField] private float playerHitFlashCount = 3;

    [Header("Screen Shake")]
    [SerializeField] private Transform cameraRoot;
    [SerializeField] private float shakeIntensity = 0.08f;
    [SerializeField] private float shakeDuration = 0.25f;

    [Header("Sorting")]
    [SerializeField] private string attackSortingLayer = "AttackAnimation";
    [SerializeField] private int attackSortingOrder = 999;

    [Header("Reveal Pulse")]
    [SerializeField] private int revealPulseCount = 2;
    [SerializeField] private float revealPulseScale = 1.12f;
    [SerializeField] private float revealPulseDuration = 0.1f;
    [SerializeField] private float revealHoldDuration = 0.3f;

    [Header("Phase 1 - Drift")]
    [SerializeField] private float phase1Duration = 0.3f;
    [SerializeField] private float phase1RotationDegrees = 15f;
    [SerializeField] private Vector2 phase1Translation = new Vector2(-0.2f, 0.2f);

    [Header("Phase 2 - Snap")]
    [SerializeField] private float phase2Duration = 0.2f;
    [SerializeField] private float phase2RotationDegrees = -30f;
    [SerializeField] private Vector3 phase2TargetScale = new Vector3(1.3f, 1.3f, 1f);

    [Header("Shake")]
    [SerializeField] private float enemyShakeDuration = 0.15f;
    [SerializeField] private float enemyShakeAngle = 10f;
    [SerializeField] private float enemyShakeSpeed = 30f;

    [Header("Phase 3 - Return")]
    [SerializeField] private float phase3Duration = 0.3f;

    [Header("Slash")]
    [SerializeField] private Sprite[] slashFrames;
    [SerializeField] private float slashFrameDuration = 0.05f;

    [Header("Player Idle Animation")]
    [SerializeField] private Sprite[] playerIdleFrames;
    [SerializeField] private float playerIdleFrameDuration = 0.12f;

    private Coroutine _playerIdleCoroutine;
    private bool _playerBusy = false;

    public static UnityAction OnSequenceComplete;

    private HashSet<CellView> _cellsInQueue = new();
    private Coroutine _screenShakeCoroutine;
    private Coroutine _playerHitCoroutine;
    private Coroutine _playerAttackCoroutine;
    private Vector2 _playerOriginalPos;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
        if (playerImage != null)
            _playerOriginalPos = playerImage.rectTransform.anchoredPosition;
    }

    private void Start()
    {
        StartIdleAnimation();
    }

    public void QueueAttack(CellView cell, EnemySpawnableSO enemy, int damage)
    {
        if (_cellsInQueue.Contains(cell)) return;
        _cellsInQueue.Add(cell);

        StartCoroutine(PlaySequence(cell, enemy, damage));
    }

    private void StartIdleAnimation()
    {
        if (_playerIdleCoroutine != null)
        {
            StopCoroutine(_playerIdleCoroutine);
        }
        _playerIdleCoroutine = StartCoroutine(PlayerIdleLoop());
    }

    private IEnumerator PlayerIdleLoop()
    {
        if (playerImage == null || playerIdleFrames == null
            || playerIdleFrames.Length == 0) yield break;

        int frame = 0;
        while (true)
        {
            if (!_playerBusy && playerImage != null)
            {
                playerImage.sprite = playerIdleFrames[frame];
                frame = (frame + 1) % playerIdleFrames.Length;
            }
            yield return new WaitForSeconds(playerIdleFrameDuration);
        }
    }

    //hell
    private IEnumerator PlaySequence(CellView cell, EnemySpawnableSO enemy, int damage)
    {
        if (cell == null) yield break;

        cell.SetSequenceLocked(true);

        Vector3 originalCellScale = cell.transform.localScale;

        for (int i = 0; i < revealPulseCount; i++)
        {
            yield return ScaleTo(cell.transform, originalCellScale,
                originalCellScale * revealPulseScale, revealPulseDuration);
            if (cell == null) yield break;
            yield return ScaleTo(cell.transform, originalCellScale * revealPulseScale,
                originalCellScale, revealPulseDuration);
            if (cell == null) yield break;
        }

        yield return new WaitForSeconds(revealHoldDuration);
        if (cell == null) yield break;

        cell.HideOccupant();

        GameObject animObj = new GameObject("AttackSprite");
        animObj.transform.SetParent(animationRoot);

        SpriteRenderer animSR = animObj.AddComponent<SpriteRenderer>();
        animSR.sprite = enemy.sprite;
        animSR.sortingLayerName = attackSortingLayer;
        animSR.sortingOrder = attackSortingOrder;

        Vector3 spawnPosition = cell.transform.position;
        animObj.transform.position = spawnPosition;
        animObj.transform.rotation = Quaternion.identity;
        Vector3 originalScale = animObj.transform.localScale;


        Vector3 phase1EndPos = spawnPosition + new Vector3(
            phase1Translation.x, phase1Translation.y, 0f);

        yield return LerpTransform(animObj.transform,
            spawnPosition, phase1EndPos,
            Quaternion.identity,
            Quaternion.Euler(0f, 0f, phase1RotationDegrees),
            phase1Duration, EaseInOut);

        if (animObj == null) yield break;

        Vector3 playerWorldPos = GetPlayerWorldPosition();
        Quaternion phase2EndRot = animObj.transform.rotation *
            Quaternion.Euler(0f, 0f, phase2RotationDegrees);

        yield return LerpTransformWithScale(animObj.transform,
            animObj.transform.position, playerWorldPos,
            animObj.transform.rotation, phase2EndRot,
            originalScale, phase2TargetScale,
            phase2Duration, EaseIn);

        if (animObj == null) yield break;


        StartCoroutine(ShakeRotation(animObj.transform,
            enemyShakeDuration, enemyShakeAngle, enemyShakeSpeed));

        if (damage > 0)
        {
            if (PlayerRunStats.Instance != null)
                PlayerRunStats.Instance.ModifyHealth(-damage);
            TriggerPlayerHitReaction();
            TriggerScreenShake();
        }

        yield return new WaitForSeconds(enemyShakeDuration);
        if (animObj == null) yield break;

        Vector3 returnPos = cell != null ? cell.transform.position : spawnPosition;

        yield return LerpTransformWithScale(animObj.transform,
            animObj.transform.position, returnPos,
            animObj.transform.rotation, Quaternion.identity,
            phase2TargetScale, originalScale,
            phase3Duration, EaseOut);


        if (cell != null)
        {
            TriggerPlayerAttackAnimation();
            yield return PlaySlashOnCell(cell, animObj);
        }
        else
        {
            Destroy(animObj);
        }

        if (cell != null)
        {
            cell.transform.localScale = originalCellScale;
            cell.SetSequenceLocked(false);
            _cellsInQueue.Remove(cell);
            CellInteractionManager.Instance?.ForceTransitionToClearedState(cell);
        }

        OnSequenceComplete?.Invoke();
    }

    private IEnumerator PlaySlashOnCell(CellView cell, GameObject enemyObj)
    {
        Destroy(enemyObj);

        if (slashFrames == null || slashFrames.Length == 0 || cell == null)
            yield break;

        GameObject slashObj = new GameObject("SlashEffect");
        slashObj.transform.SetParent(animationRoot);
        slashObj.transform.position = cell.transform.position;

        SpriteRenderer slashSR = slashObj.AddComponent<SpriteRenderer>();
        slashSR.sortingLayerName = attackSortingLayer;
        slashSR.sortingOrder = attackSortingOrder + 1;

        foreach (var frame in slashFrames)
        {
            if (slashSR == null) break;
            slashSR.sprite = frame;
            yield return new WaitForSeconds(slashFrameDuration);
        }

        Destroy(slashObj);
    }

    private void TriggerPlayerHitReaction()
    {
        if (playerImage == null) return;
        if (_playerHitCoroutine != null) StopCoroutine(_playerHitCoroutine);
        _playerHitCoroutine = StartCoroutine(PlayerHitRoutine());
    }

    private IEnumerator PlayerHitRoutine()
    {
        if (playerImage == null) yield break;

        _playerBusy = true;

        RectTransform rt = playerImage.rectTransform;
        Vector2 originalPos = _playerOriginalPos;

        if (playerHitSprite != null)
            playerImage.sprite = playerHitSprite;

        Vector2 knockbackPos = originalPos + new Vector2(playerHitKnockback, 0f);
        float elapsed = 0f;
        float knockDuration = playerHitDuration * 0.3f;

        while (elapsed < knockDuration)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(originalPos, knockbackPos,
                elapsed / knockDuration);
            yield return null;
        }

        for (int i = 0; i < playerHitFlashCount; i++)
        {
            playerImage.color = Color.red;
            yield return new WaitForSeconds(
                playerHitDuration / (playerHitFlashCount * 2f));
            playerImage.color = Color.white;
            yield return new WaitForSeconds(
                playerHitDuration / (playerHitFlashCount * 2f));
        }

        elapsed = 0f;
        while (elapsed < knockDuration)
        {
            elapsed += Time.deltaTime;
            rt.anchoredPosition = Vector2.Lerp(knockbackPos, originalPos,
                elapsed / knockDuration);
            yield return null;
        }

        rt.anchoredPosition = originalPos;

        if (playerIdleSprite != null)
            playerImage.sprite = playerIdleSprite;

        playerImage.color = Color.white;
        _playerBusy = false;
    }

    private void TriggerPlayerAttackAnimation()
    {
        if (playerImage == null || playerAttackFrames == null
            || playerAttackFrames.Length == 0) return;
        if (_playerAttackCoroutine != null) StopCoroutine(_playerAttackCoroutine);
        _playerAttackCoroutine = StartCoroutine(PlayerAttackRoutine());
    }

    private IEnumerator PlayerAttackRoutine()
    {
        _playerBusy = true;
        foreach (var frame in playerAttackFrames)
        {
            if (playerImage == null) yield break;
            playerImage.sprite = frame;
            yield return new WaitForSeconds(playerAttackFrameDuration);
        }

        if (playerImage != null && playerIdleSprite != null)
            playerImage.sprite = playerIdleSprite;
        _playerBusy = false;
    }

    private void TriggerScreenShake()
    {
        if (cameraRoot == null) return;
        if (_screenShakeCoroutine != null) StopCoroutine(_screenShakeCoroutine);
        _screenShakeCoroutine = StartCoroutine(ScreenShakeRoutine());
    }

    private IEnumerator ScreenShakeRoutine()
    {
        Vector3 originalPos = cameraRoot.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float strength = Mathf.Lerp(shakeIntensity, 0f, elapsed / shakeDuration);
            cameraRoot.localPosition = originalPos + new Vector3(
                Random.Range(-strength, strength),
                Random.Range(-strength, strength), 0f);
            yield return null;
        }

        cameraRoot.localPosition = originalPos;
    }

    private Vector3 GetPlayerWorldPosition()
    {
        if (playerUITransform == null) return Vector3.zero;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                null, playerUITransform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                screenPos.x, screenPos.y,
                Mathf.Abs(mainCamera.transform.position.z)));
            worldPos.z = 0f;
            return worldPos;
        }
        else
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera, playerUITransform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
                screenPos.x, screenPos.y,
                Mathf.Abs(mainCamera.transform.position.z)));
            worldPos.z = 0f;
            return worldPos;
        }
    }

    private IEnumerator ShakeRotation(Transform t, float duration,
        float angle, float speed)
    {
        if (t == null) yield break;
        Quaternion baseRot = t.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            float envelope = 1f - Mathf.Clamp01(elapsed / duration);
            t.rotation = baseRot * Quaternion.Euler(0f, 0f,
                Mathf.Sin(elapsed * speed * Mathf.PI * 2f) * angle * envelope);
            yield return null;
        }
        if (t != null) t.rotation = baseRot;
    }

    private IEnumerator LerpTransform(Transform t,
        Vector3 fromPos, Vector3 toPos,
        Quaternion fromRot, Quaternion toRot,
        float duration, System.Func<float, float> easingFn)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            float e = easingFn(Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.Lerp(fromPos, toPos, e);
            t.rotation = Quaternion.Slerp(fromRot, toRot, e);
            yield return null;
        }
        if (t != null) { t.position = toPos; t.rotation = toRot; }
    }

    private IEnumerator LerpTransformWithScale(Transform t,
        Vector3 fromPos, Vector3 toPos,
        Quaternion fromRot, Quaternion toRot,
        Vector3 fromScale, Vector3 toScale,
        float duration, System.Func<float, float> easingFn)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            float e = easingFn(Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.Lerp(fromPos, toPos, e);
            t.rotation = Quaternion.Slerp(fromRot, toRot, e);
            t.localScale = Vector3.Lerp(fromScale, toScale, e);
            yield return null;
        }
        if (t != null)
        {
            t.position = toPos;
            t.rotation = toRot;
            t.localScale = toScale;
        }
    }

    private IEnumerator ScaleTo(Transform t, Vector3 from, Vector3 to,
        float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (t == null) yield break;
            elapsed += Time.deltaTime;
            t.localScale = Vector3.Lerp(from, to,
                Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        if (t != null) t.localScale = to;
    }

    private static float EaseInOut(float t) =>
        t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    private static float EaseIn(float t) => t * t;
    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}