using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class DrunkVisionEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Material drunkMaterial;
    [SerializeField] private RawImage fullscreenOverlay;

    [Header("Distortion Settings")]
    [SerializeField] private float waveAmplitude = 0.008f;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float waveSpeed = 1.2f;
    [SerializeField] private float vignetteIntensity = 0.3f;

    [Header("Trail Settings")]
    [SerializeField] private float trailStrength = 0.15f;

    private bool _isDrunk = false;
    private float _currentIntensity = 0f;
    private Coroutine _fadeCoroutine;

    private void OnEnable()
    {
        DrunkStateManager.OnDrunkStart += OnDrunkStart;
        DrunkStateManager.OnSobered += OnSobered;
    }

    private void OnDisable()
    {
        DrunkStateManager.OnDrunkStart -= OnDrunkStart;
        DrunkStateManager.OnSobered -= OnSobered;
    }

    private void OnDrunkStart()
    {
        _isDrunk = true;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeEffect(0f, 1f, 2f));
    }

    private void OnSobered()
    {
        _isDrunk = false;
        if (_fadeCoroutine != null) StopCoroutine(_fadeCoroutine);
        _fadeCoroutine = StartCoroutine(FadeEffect(_currentIntensity, 0f, 1.5f));
    }

    private void Update()
    {
        if (drunkMaterial == null || !_isDrunk && _currentIntensity <= 0f) return;

        drunkMaterial.SetFloat("_WaveAmplitude",
            waveAmplitude * _currentIntensity);
        drunkMaterial.SetFloat("_WaveFrequency",
            waveFrequency);
        drunkMaterial.SetFloat("_WaveSpeed",
            waveSpeed);
        drunkMaterial.SetFloat("_Time",
            Time.time);
        drunkMaterial.SetFloat("_Vignette",
            vignetteIntensity * _currentIntensity);
        drunkMaterial.SetFloat("_TrailStrength",
            trailStrength * _currentIntensity);
    }

    private IEnumerator FadeEffect(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            _currentIntensity = Mathf.Lerp(from, to, elapsed / duration);
            if (fullscreenOverlay != null)
                fullscreenOverlay.color = new Color(1f, 1f, 1f, _currentIntensity);
            yield return null;
        }
        _currentIntensity = to;
    }
}