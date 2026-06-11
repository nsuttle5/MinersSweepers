using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class AttackAnimationManager : MonoBehaviour
{
    public static AttackAnimationManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private RectTransform playerUITransform;
    [SerializeField] private Canvas canvas;
    [SerializeField] private Transform animationRoot;
    [SerializeField] private Camera mainCamera;

    [Header("Sorting")]
    [SerializeField] private string attackSortingLayer = "AttackAnimation";
    [SerializeField] private int attackSortingOrder = 999;

    [Header("Slow Drift")]
    [SerializeField] private float phase1Duration = 0.4f;
    [SerializeField] private float phase1RotationDegrees = 20f;
    [SerializeField] private Vector2 phase1Translation = new Vector2(-0.3f, 0.3f);

    [Header("Fast Snap")]
    [SerializeField] private float phase2Duration = 0.2f;
    [SerializeField] private float phase2RotationDegrees = -40f;
    [SerializeField] private Vector3 phase2TargetScale = new Vector3(1.4f, 1.4f, 1f);

    [Header("Shake")]
    [SerializeField] private float shakeDuration = 0.3f;
    [SerializeField] private float shakeAngle = 12f;
    [SerializeField] private float shakeSpeed = 25f;

    [Header("Return")]
    [SerializeField] private float phase3Duration = 0.4f;

    [Header("Wait")]
    [SerializeField] private float waitTime = 0f;

    public static UnityAction OnAttackAnimationComplete;

    private Queue<AttackAnimationRequest> _queue = new();
    private bool _isPlaying = false;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (mainCamera == null) mainCamera = Camera.main;
    }

    public void QueueAttack(Sprite enemySprite, int damage, Vector3 startWorldPosition)
    {
        var request = new AttackAnimationRequest(enemySprite, damage);
        request.startWorldPosition = startWorldPosition;
        _queue.Enqueue(request);
        if (!_isPlaying)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _isPlaying = true;
        while (_queue.Count > 0)
        {
            AttackAnimationRequest request = _queue.Dequeue();
            yield return StartCoroutine(PlayAttack(request));
        }
        _isPlaying = false;
    }

    private IEnumerator PlayAttack(AttackAnimationRequest request)
    {
        GameObject animObj = new GameObject("AttackSprite");
        animObj.transform.SetParent(animationRoot);

        SpriteRenderer animSR = animObj.AddComponent<SpriteRenderer>();
        animSR.sprite = request.enemySprite;
        animSR.sortingLayerName = attackSortingLayer;
        animSR.sortingOrder = attackSortingOrder;

        animObj.transform.position = request.startWorldPosition;
        animObj.transform.rotation = Quaternion.identity;

        Vector3 spawnPosition = animObj.transform.position;
        Quaternion spawnRotation = animObj.transform.rotation;
        Vector3 originalScale = animObj.transform.localScale;

        yield return new WaitForSeconds(waitTime);

        // Phase 1 - drift
        Vector3 phase1EndPos = spawnPosition + new Vector3(phase1Translation.x, phase1Translation.y, 0f);
        Quaternion phase1EndRot = spawnRotation * Quaternion.Euler(0f, 0f, phase1RotationDegrees);

        yield return LerpTransform(
            animObj.transform,
            spawnPosition, phase1EndPos,
            spawnRotation, phase1EndRot,
            phase1Duration, EaseInOut);

        // Phase 2 - snap to player world position
        Vector3 playerWorldPos = GetPlayerWorldPosition();
        Quaternion phase2EndRot = animObj.transform.rotation * Quaternion.Euler(0f, 0f, phase2RotationDegrees);

        yield return LerpTransformWithScale(
            animObj.transform,
            animObj.transform.position, playerWorldPos,
            animObj.transform.rotation, phase2EndRot,
            originalScale, phase2TargetScale,
            phase2Duration, EaseIn);

        // Shake
        yield return ShakeRotation(animObj.transform, shakeDuration, shakeAngle, shakeSpeed);

        // Apply damage after sprite reaches player
        if (PlayerRunStats.Instance != null)
            PlayerRunStats.Instance.ModifyHealth(-request.damage);

        OnAttackAnimationComplete?.Invoke();

        // Phase 3 - return
        yield return LerpTransformWithScale(
            animObj.transform,
            animObj.transform.position, spawnPosition,
            animObj.transform.rotation, spawnRotation,
            phase2TargetScale, originalScale,
            phase3Duration, EaseOut);

        Destroy(animObj);
    }

    private Vector3 GetPlayerWorldPosition()
    {
        if (playerUITransform == null) return Vector3.zero;

        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(null, playerUITransform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
            worldPos.z = 0f;
            return worldPos;
        }
        else
        {
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, playerUITransform.position);
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Mathf.Abs(mainCamera.transform.position.z)));
            worldPos.z = 0f;
            return worldPos;
        }
    }

    private IEnumerator ShakeRotation(Transform t, float duration, float angle, float speed)
    {
        Quaternion baseRot = t.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float envelope = 1f - Mathf.Clamp01(elapsed / duration);
            float offsetDeg = Mathf.Sin(elapsed * speed * Mathf.PI * 2f) * angle * envelope;
            t.rotation = baseRot * Quaternion.Euler(0f, 0f, offsetDeg);
            yield return null;
        }

        t.rotation = baseRot;
    }

    private IEnumerator LerpTransform(
        Transform t,
        Vector3 fromPos, Vector3 toPos,
        Quaternion fromRot, Quaternion toRot,
        float duration, System.Func<float, float> easingFn)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float e = easingFn(Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.Lerp(fromPos, toPos, e);
            t.rotation = Quaternion.Slerp(fromRot, toRot, e);
            yield return null;
        }
        t.position = toPos;
        t.rotation = toRot;
    }

    private IEnumerator LerpTransformWithScale(
        Transform t,
        Vector3 fromPos, Vector3 toPos,
        Quaternion fromRot, Quaternion toRot,
        Vector3 fromScale, Vector3 toScale,
        float duration, System.Func<float, float> easingFn)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float e = easingFn(Mathf.Clamp01(elapsed / duration));
            t.position = Vector3.Lerp(fromPos, toPos, e);
            t.rotation = Quaternion.Slerp(fromRot, toRot, e);
            t.localScale = Vector3.Lerp(fromScale, toScale, e);
            yield return null;
        }
        t.position = toPos;
        t.rotation = toRot;
        t.localScale = toScale;
    }

    private static float EaseInOut(float t) =>
        t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;
    private static float EaseIn(float t) => t * t;
    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}

public class AttackAnimationRequest
{
    public Sprite enemySprite;
    public int damage;
    public Vector3 startWorldPosition;

    public AttackAnimationRequest(Sprite sprite, int damage)
    {
        this.enemySprite = sprite;
        this.damage = damage;
    }
}