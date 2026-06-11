using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;
using TMPro;

public class FishingCell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public FishingCellType cellType;
    public bool isRevealed = false;
    public bool isDestroyed = false;
    public bool isMarked = false;

    [Header("Sprites")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite fishSprite;
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite bombSprite;
    [SerializeField] private Sprite markedSprite;

    [Header("Hover")]
    [SerializeField] private float hoverScale = 1.1f;
    [SerializeField] private float hoverDuration = 0.08f;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private float hoverSoundCooldown = 0.08f;

    [Header("Damage Text")]
    [SerializeField] private TextMeshPro neighborBombText;

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
        isMarked = false;
        transform.localScale = _originalScale;
        UpdateVisual();
    }

    public void Reveal()
    {
        if (isRevealed || isDestroyed) return;
        isRevealed = true;
        isMarked = false;
        UpdateVisual();
    }

    public void ToggleMark()
    {
        if (isRevealed || isDestroyed) return;
        isMarked = !isMarked;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if (!sr) return;

        if (!isRevealed)
        {
            sr.sprite = isMarked ? markedSprite : hiddenSprite;
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
        if (isRevealed || isDestroyed) return;

        if (eventData.button == PointerEventData.InputButton.Left)
            OnCellClicked?.Invoke(this);
        else if (eventData.button == PointerEventData.InputButton.Right)
            ToggleMark();
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

    public void SetNeighborBombCount(int count)
    {
        if (!neighborBombText) return;

        if (isRevealed && count > 0)
        {
            neighborBombText.text = count.ToString();
            neighborBombText.gameObject.SetActive(true);
        }
        else
        {
            neighborBombText.gameObject.SetActive(false);
        }
    }
}

public enum FishingCellType { Empty, Fish, Bomb }