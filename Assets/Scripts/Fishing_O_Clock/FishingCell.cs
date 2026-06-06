using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class FishingCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public FishingCellType cellType;
    public bool isRevealed = false;
    public bool isDestroyed = false;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite fishSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite bombSprite;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.08f;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float hoverSoundCooldown = 0.08f;

    private Vector3 _originalScale;
    private Coroutine _hoverCoroutine;
    private static float _lastHoverSoundTime = -999f;

    public static UnityAction<FishingCell> OnCellClicked;

    private void Awake()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        _originalScale = transform.localScale;
    }

    public void Setup(FishingCellType type)
    {
        cellType = type;
        isRevealed = false;
        isDestroyed = false;
        transform.localScale = _originalScale;
        UpdateVisual();
    }

    public void Reveal()
    {
        if (isRevealed || isDestroyed) return;
        isRevealed = true;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (!sr) return;

        if (!isRevealed)
        {
            sr.sprite = hiddenSprite;
            return;
        }

        switch (cellType)
        {
            case FishingCellType.Empty: sr.sprite = emptySprite; break;
            case FishingCellType.Fish: sr.sprite = fishSprite; break;
            case FishingCellType.Bomb: sr.sprite = bombSprite; break;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;
        if (isRevealed || isDestroyed) return;
        OnCellClicked?.Invoke(this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_hoverCoroutine != null) StopCoroutine(_hoverCoroutine);
        _hoverCoroutine = StartCoroutine(ScaleTo(transform.localScale, _originalScale * hoverScale, hoverDuration));

        if (hoverSound != null && audioSource != null && Time.time - _lastHoverSoundTime > hoverSoundCooldown)
        {
            audioSource.PlayOneShot(hoverSound);
            _lastHoverSoundTime = Time.time;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_hoverCoroutine != null) StopCoroutine(_hoverCoroutine);
        _hoverCoroutine = StartCoroutine(ScaleTo(transform.localScale, _originalScale, hoverDuration));
    }

    private IEnumerator ScaleTo(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.localScale = Vector3.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        transform.localScale = to;
        _hoverCoroutine = null;
    }
}

public enum FishingCellType { Empty, Fish, Bomb }