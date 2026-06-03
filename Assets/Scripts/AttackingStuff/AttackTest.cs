using System.Collections;
using UnityEngine;

public class SpriteAnimationTrigger : MonoBehaviour
{
    [Header("Target & Spawn")]
    public Transform targetTransform;
    private Vector3 spawnPosition;
    private Quaternion spawnRotation;

    [Header("Wait Time")]
    public float waitTime = 0.5f;

    [Header("Slow drift (CCW + top-left)")]
    public float phase1Duration = 1.5f;
    public float phase1RotationDegrees = 30f;
    public Vector2 phase1Translation = new Vector2(-0.5f, 0.5f);

    [Header("Fast snap (CW + fly to target)")]
    public float phase2Duration = 0.3f;
    public float phase2RotationDegrees = -60f;

    [Header("Shake on arrival")]
    public float shakeDuration = 0.4f;
    public float shakeAngle = 15f;
    public float shakeSpeed = 25f;

    [Header("Scale (applied to parent)")]
    public Vector3 phase2TargetScale = new Vector3(1.5f, 1.5f, 1f);
    private Vector3 _originalParentScale;

    [Header("Return to spawn")]
    public float phase3Duration = 0.6f;

    private bool _isPlaying = false;


    void Start()
    {
        spawnPosition = transform.position;
        spawnRotation = transform.rotation;
        _originalParentScale = transform.localScale;
    }

    public void Trigger()
    {
        if (!_isPlaying)
            StartCoroutine(PlaySequence());
    }

    void OnMouseDown() => Trigger();



    private IEnumerator PlaySequence()
    {
        _isPlaying = true;

        yield return new WaitForSeconds(waitTime);

        Vector3 phase1EndPos = spawnPosition + new Vector3(phase1Translation.x, phase1Translation.y, 0f);
        Quaternion phase1EndRot = spawnRotation * Quaternion.Euler(0f, 0f, phase1RotationDegrees);

        yield return LerpTransform(
            transform.position, phase1EndPos,
            transform.rotation, phase1EndRot,
            phase1Duration, EaseInOut);

        Vector3 phase2EndPos = targetTransform.position;
        Quaternion phase2EndRot = transform.rotation * Quaternion.Euler(0f, 0f, phase2RotationDegrees);

        yield return LerpTransformWithScale(
            transform.position, phase2EndPos,
            transform.rotation, phase2EndRot,
            _originalParentScale, phase2TargetScale,
            phase2Duration, EaseIn);

        yield return ShakeRotation(shakeDuration, shakeAngle, shakeSpeed);

        yield return LerpTransformWithScale(
            transform.position, spawnPosition,
            transform.rotation, spawnRotation,
            phase2TargetScale, _originalParentScale,
            phase3Duration, EaseOut);

        _isPlaying = false;
    }

    private IEnumerator ShakeRotation(float duration, float angle, float speed)
    {
        Quaternion baseRot = transform.rotation;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float envelope = 1f - Mathf.Clamp01(elapsed / duration);
            float offsetDeg = Mathf.Sin(elapsed * speed * Mathf.PI * 2f) * angle * envelope;

            transform.rotation = baseRot * Quaternion.Euler(0f, 0f, offsetDeg);

            yield return null;
        }

        transform.rotation = baseRot;
    }

    private IEnumerator LerpTransform(
        Vector3 fromPos, Vector3 toPos,
        Quaternion fromRot, Quaternion toRot,
        float duration, System.Func<float, float> easingFn)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float e = easingFn(Mathf.Clamp01(elapsed / duration));

            transform.position = Vector3.Lerp(fromPos, toPos, e);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, e);

            yield return null;
        }

        transform.position = toPos;
        transform.rotation = toRot;
    }

    private IEnumerator LerpTransformWithScale(
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

            transform.position = Vector3.Lerp(fromPos, toPos, e);
            transform.rotation = Quaternion.Slerp(fromRot, toRot, e);
            transform.localScale = Vector3.Lerp(fromScale, toScale, e);

            yield return null;
        }

        transform.position = toPos;
        transform.rotation = toRot;
        transform.localScale = toScale;
    }

    private static float EaseInOut(float t) =>
        t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) / 2f;

    private static float EaseIn(float t) => t * t;
    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}