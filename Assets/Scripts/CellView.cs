using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System.Collections;

public class CellView : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public int x, y;
    public SpawnableSO spawnable;
    public BoardManager boardManager;
    private SpriteRenderer sr;

    public bool Revealed => State != CellState.Hidden;

    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite loneSprite;
    [SerializeField] private Sprite edgeSprite;
    [SerializeField] private Sprite revealedSprite;

    [SerializeField] private TextMeshPro markText;
    [SerializeField] private TextMeshPro damageText;
    [SerializeField] private SpriteRenderer occupantSR;

    public CellState State { get; private set; } = CellState.Hidden;
    public bool WasDirectlyClicked { get; private set; } = false;

    private bool isPartialRevealed = false;
    public bool IsActiveThreat => (State == CellState.Hidden || State == CellState.Revealed) && spawnable != null && spawnable.type == SpawnableType.Enemy;

    public static UnityAction<CellView, Vector2> OnCellRightClick;

    public SpawnableSO spawnableBeforeAbilities { get; set; }

    [Header("Audio Shit")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverDuration = 0.1f;
    [SerializeField] private AudioClip hoverSound;
    [SerializeField] private AudioSource audioSource;
    private Vector3 originalScale;
    private Coroutine hoverCoroutine;
    private float lastHoverTime = -999f;
    [SerializeField] private float hoverCooldown = 0.05f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!damageText) Debug.LogError("DamageText not set on CellView prefab");
        if (!markText) Debug.LogError("MarkText not set on CellView prefab");
        if (!occupantSR) Debug.LogError("OccupantSR not set on CellView prefab");

        if (markText) markText.sortingOrder = sr.sortingOrder + 1;
        if (damageText) damageText.sortingOrder = sr.sortingOrder + 1;
        if (occupantSR) occupantSR.sortingOrder = sr.sortingOrder + 1;

        if (damageText) damageText.gameObject.SetActive(false);
        if (markText) markText.gameObject.SetActive(false);
        if (occupantSR) occupantSR.gameObject.SetActive(false);

        originalScale = transform.localScale;
    }

    public void SetPartialReveal(bool active)
    {
        isPartialRevealed = active;
        UpdateVisual();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (boardManager != null)
            {
                boardManager.OnCellClicked(x, y);
            }
        }
        else if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (Revealed) return;

            OnCellRightClick?.Invoke(this, transform.position);
        }
    }

    public void Mark(string symbol)
    {
        if (symbol == string.Empty)
        {
            markText.gameObject.SetActive(false);
            return;
        }

        markText.text = symbol;
        markText.gameObject.SetActive(true);
    }

    public void SetState(CellState newState, bool directClickOverride = false)
    {
        State = newState;
        if (newState == CellState.Revealed) WasDirectlyClicked = directClickOverride;

        if (boardManager != null && newState == CellState.Revealed) boardManager.NotifyCellRevealed(this);

        UpdateVisual();
    }

    public void Reveal(bool wasDirectClick = true, bool triggerAbilities = true)
    {
        if (Revealed) return;

        SetState(CellState.Revealed, wasDirectClick);

        if (spawnable != null)
        {
            LogbookManager.Instance.Discover(spawnable);

            spawnableBeforeAbilities = spawnable;

            if (triggerAbilities && spawnable.abilities != null)
            {
                foreach (var ability in spawnable.abilities)
                {
                    if (ability != null) ability.OnReveal(this, boardManager);
                }
            }
        }

        UpdateVisual();
        if (markText) markText.gameObject.SetActive(false);
    }

    public void UpdateVisual()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = y;

        if (occupantSR) occupantSR.sortingOrder = sr.sortingOrder + 1;
        if (markText) markText.sortingOrder = sr.sortingOrder + 2;
        if (damageText) damageText.sortingOrder = sr.sortingOrder + 2;

        if (occupantSR) occupantSR.gameObject.SetActive(false);

        switch (State)
        {
            case CellState.Hidden:
                if (boardManager != null && y == boardManager.Height - 1 && edgeSprite != null)
                    sr.sprite = edgeSprite;
                else if (boardManager != null && boardManager.IsSurroundedByRevealed(x, y) && loneSprite != null)
                    sr.sprite = loneSprite;
                else if (isPartialRevealed && edgeSprite != null)
                    sr.sprite = edgeSprite;
                else if (hiddenSprite != null)
                    sr.sprite = hiddenSprite;

                if (damageText) damageText.gameObject.SetActive(false);
                break;
            case CellState.Revealed:
                if (revealedSprite != null) sr.sprite = revealedSprite;
                HandleOccupantVisual();
                break;
            case CellState.Interacted:
                if (revealedSprite != null) sr.sprite = revealedSprite;
                HandleOccupantVisual();
                break;
            case CellState.Cleared:
                if (revealedSprite != null) sr.sprite = revealedSprite;
                break;
        }

        sr.color = Color.white;
        TryDisplaySurroundingDamage();
    }

    private void HandleOccupantVisual()
    {
        if (!occupantSR || spawnable == null) return;

        Sprite activeOccupantSprite = null;

        if (State == CellState.Interacted && spawnable is EnemySpawnableSO enemy && enemy.interactedSprite != null)
        {
            activeOccupantSprite = enemy.interactedSprite;
        }
        else
        {
            activeOccupantSprite = spawnable.sprite;
        }

        if (activeOccupantSprite != null)
        {
            occupantSR.sprite = activeOccupantSprite;
            occupantSR.gameObject.SetActive(true);
        }
    }


    public void TryDisplaySurroundingDamage()
    {
        if (State == CellState.Hidden || isPartialRevealed)
        {
            if (damageText) damageText.gameObject.SetActive(false);
            return;
        }

        bool canShowNumber = (spawnable == null) ||
                             (State == CellState.Cleared) ||
                             (State == CellState.Interacted);

        if (boardManager != null && canShowNumber)
        {
            int surroundingDamage = boardManager.GetNeighborDamage(x, y);
            if (damageText)
            {
                if (surroundingDamage > 0)
                {
                    damageText.text = surroundingDamage.ToString();
                    damageText.gameObject.SetActive(true);
                }
                else
                {
                    damageText.text = "";
                    damageText.gameObject.SetActive(false);
                }
            }
        }
        else
        {
            if (damageText) damageText.gameObject.SetActive(false);
        }

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(ScaleTo(transform.localScale, originalScale * hoverScale, hoverDuration));

        if (hoverSound != null && audioSource != null && Time.time - lastHoverTime >= hoverCooldown)
        {
            audioSource.PlayOneShot(hoverSound);
            lastHoverTime = Time.time;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (hoverCoroutine != null) StopCoroutine(hoverCoroutine);
        hoverCoroutine = StartCoroutine(ScaleTo(transform.localScale, originalScale, hoverDuration));
    }

    private IEnumerator ScaleTo(Vector3 from, Vector3 to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localScale = to;
        hoverCoroutine = null;
    }
}

public enum CellState { Hidden, Revealed, Interacted, Cleared }