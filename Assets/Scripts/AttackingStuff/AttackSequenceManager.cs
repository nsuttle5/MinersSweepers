// AttackSequenceManager.cs
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

    [Header("Sorting")]
    [SerializeField] private string attackSortingLayer = "AttackAnimation";
    [SerializeField] private int attackSortingOrder = 999;

    [Header("Reveal Pause")]
    [SerializeField] private float revealHoldDuration = 0.4f;
    [SerializeField] private int revealPulseCount = 2;
    [SerializeField] private float revealPulseScale = 1.15f;
    [SerializeField] private float revealPulseDuration = 0.1f;

    [Header("Phase 1_Drift")]
    [SerializeField] private float phase1Duration = 0.35f;
    [SerializeField] private float phase1RotationDegrees = 20f;
    [SerializeField] private Vector2 phase1Translation = new Vector2(-0.3f, 0.3f);

    [Header("Phase 2_Snap to Player")]
    [SerializeField] private float phase2Duration = 0.2f;
    [SerializeField] private float phase2RotationDegrees = -40f;
    [SerializeField] private Vector3 phase2TargetScale = new Vector3(1.4f, 1.4f, 1f);

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.25f;
    [SerializeField] private float shakeAngle = 12f;
    [SerializeField] private float shakeSpeed = 25f;

    [Header("Phase 3_Return")]
    [SerializeField] private float phase3Duration = 0.35f;

    [Header("Slash")]
    [SerializeField] private Sprite[] slashFrames;
    [SerializeField] private float slashFrameDuration = 0.05f;
    [SerializeField] private float slashHoldDuration = 0.15f;

    public static UnityAction OnSequenceComplete;

    private Queue<AttackSequenceRequest> _queue = new();
    private bool _isPlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void QueueAttack(CellView cell, EnemySpawnableSO enemy, int damage)
    {
        _queue.Enqueue(new AttackSequenceRequest(cell, enemy, damage));
        if (!_isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _isPlaying = true;
        while (_queue.Count > 0)
        {
            AttackSequenceRequest request = _queue.Dequeue();
            yield return StartCoroutine(PlaySequence(request));
        }
        _isPlaying = false;
    }

    //hell
    private IEnumerator PlaySequence(AttackSequenceRequest request)
    {
        CellView cell = request.cell;
        if (cell == null) yield break;

        Vector3 originalCellScale = cell.transform.localScale;
        for (int i = 0; i < revealPulseCount; i++)
        {
            yield return ScaleTo(cell.transform, originalCellScale,
                originalCellScale * revealPulseScale, revealPulseDuration);
            yield return ScaleTo(cell.transform, originalCellScale * revealPulseScale,
                originalCellScale, revealPulseDuration);
        }

        yield return new WaitForSeconds(revealHoldDuration);

        if (cell == null) yield break;

        Vector3 startWorldPos = cell.transform.position;

        cell.HideOccupant();

        GameObject animObj = new GameObject("AttackSprite");
        animObj.transform.SetParent(animationRoot);

        SpriteRenderer animSR = animObj.AddComponent<SpriteRenderer>();
        animSR.sprite = request.enemy.sprite;
        animSR.sortingLayerName = attackSortingLayer;
        animSR.sortingOrder = attackSortingOrder;

        animObj.transform.position = startWorldPos;
        animObj.transform.rotation = Quaternion.identity;

        Vector3 spawnPosition = animObj.transform.position;
        Quaternion spawnRotation = animObj.transform.rotation;
        Vector3 originalScale = animObj.transform.localScale;

        Vector3 phase1EndPos = spawnPosition + new Vector3(
            phase1Translation.x, phase1Translation.y, 0f);
        Quaternion phase1EndRot = spawnRotation *
            Quaternion.Euler(0f, 0f, phase1RotationDegrees);

        yield return LerpTransform(animObj.transform,
            spawnPosition, phase1EndPos,
            spawnRotation, phase1EndRot,
            phase1Duration, EaseInOut);

        Vector3 playerWorldPos = GetPlayerWorldPosition();
        Quaternion phase2EndRot = animObj.transform.rotation *
            Quaternion.Euler(0f, 0f, phase2RotationDegrees);

        yield return LerpTransformWithScale(animObj.transform,
            animObj.transform.position, playerWorldPos,
            animObj.transform.rotation, phase2EndRot,
            originalScale, phase2TargetScale,
            phase2Duration, EaseIn);

        yield return ShakeRotation(animObj.transform,
            shakeDuration, shakeAngle, shakeSpeed);

        if (PlayerRunStats.Instance != null)
            PlayerRunStats.Instance.ModifyHealth(-request.damage);

        yield return LerpTransformWithScale(animObj.transform,
            animObj.transform.position, spawnPosition,
            animObj.transform.rotation, spawnRotation,
            phase2TargetScale, originalScale,
            phase3Duration, EaseOut);

        Destroy(animObj);

        if (cell != null)
            yield return PlaySlashOnCell(cell);


        if (cell != null)
        {
            cell.transform.localScale = originalCellScale;
            CellInteractionManager.Instance.ForceTransitionToClearedState(cell);
        }

        OnSequenceComplete?.Invoke();
    }

    private IEnumerator PlaySlashOnCell(CellView cell)
    {
        if (slashFrames == null || slashFrames.Length == 0 || cell == null)
        {
            yield return new WaitForSeconds(slashHoldDuration);
            yield break;
        }

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

        yield return new WaitForSeconds(slashHoldDuration);
        Destroy(slashObj);
    }

    private Vector3 GetPlayerWorldPosition()
    {
        if (playerUITransform == null) return Vector3.zero;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                null, playerUITransform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y,
                    Mathf.Abs(mainCamera.transform.position.z)));
            worldPos.z = 0f;
            return worldPos;
        }
        else
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
                canvas.worldCamera, playerUITransform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                new Vector3(screenPos.x, screenPos.y,
                    Mathf.Abs(mainCamera.transform.position.z)));
            worldPos.z = 0f;
            return worldPos;
        }
    }

    private IEnumerator ShakeRotation(Transform t, float duration,
        float angle, float speed)
    {
        Quaternion baseRot = t.rotation;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float envelope = 1f - Mathf.Clamp01(elapsed / duration);
            float offsetDeg = Mathf.Sin(elapsed * speed * Mathf.PI * 2f)
                * angle * envelope;
            t.rotation = baseRot * Quaternion.Euler(0f, 0f, offsetDeg);
            yield return null;
        }
        t.rotation = baseRot;
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

public class AttackSequenceRequest
{
    public CellView cell;
    public EnemySpawnableSO enemy;
    public int damage;
    public Vector3 startWorldPosition;

    public AttackSequenceRequest(CellView cell, EnemySpawnableSO enemy,
        int damage)
    {
        this.cell = cell;
        this.enemy = enemy;
        this.damage = damage;
        this.startWorldPosition = cell != null ? cell.transform.position
            : Vector3.zero;
    }
}