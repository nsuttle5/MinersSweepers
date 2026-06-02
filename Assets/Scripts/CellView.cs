using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class CellView : MonoBehaviour, IPointerClickHandler
{
    public int x, y;
    public SpawnableSO spawnable;
    public BoardManager boardManager;
    private SpriteRenderer sr;

    public bool Revealed => State != CellState.Hidden;

    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite revealedSprite;

    [SerializeField] private TextMeshPro markText;
    [SerializeField] private TextMeshPro damageText;

    public CellState State { get; private set; } = CellState.Hidden;
    public bool WasDirectlyClicked { get; private set; } = false;

    public bool IsActiveThreat => (State == CellState.Hidden || State == CellState.Revealed) && spawnable != null && spawnable.type == SpawnableType.Enemy;

    public static UnityAction<CellView, Vector2> OnCellRightClick;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!damageText) Debug.LogError("DamageText not set on CellView prefab");
        if (!markText) Debug.LogError("MarkText not set on CellView prefab");

        if (markText)
        {
            markText.sortingOrder = sr.sortingOrder + 1;
        }

        if (damageText)
        {
            damageText.sortingOrder = sr.sortingOrder + 1;
        }

        if (damageText) damageText.gameObject.SetActive(false);
        if (markText) markText.gameObject.SetActive(false);
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

            if (triggerAbilities && spawnable.abilities != null)
            {
                foreach (var ability in spawnable.abilities)
                {
                    if (ability != null) ability.OnReveal(this, boardManager);
                }
            }
        }

        if (markText) markText.gameObject.SetActive(false);
    }

    public void UpdateVisual()
    {
        if (!sr) sr = GetComponent<SpriteRenderer>();
        sr.sortingOrder = y;

        if (markText)
        {
            markText.sortingOrder = sr.sortingOrder + 1;
        }

        if (damageText)
        {
            damageText.sortingOrder = sr.sortingOrder + 1;
        }

        switch (State)
        {
            case CellState.Hidden:
                if (hiddenSprite != null) sr.sprite = hiddenSprite;
                if (damageText) damageText.gameObject.SetActive(false);
                break;
            case CellState.Revealed:
                if (spawnable != null && spawnable.sprite != null)
                    sr.sprite = spawnable.sprite;
                else if (revealedSprite != null)
                    sr.sprite = revealedSprite;
                break;
            case CellState.Interacted:
                if (spawnable is EnemySpawnableSO enemy && enemy.interactedSprite != null)
                    sr.sprite = enemy.interactedSprite;
                else if (revealedSprite != null)
                    sr.sprite = revealedSprite;
                break;
            case CellState.Cleared:
                if (revealedSprite != null) sr.sprite = revealedSprite;
                break;
        }

        sr.color = Color.white;
        TryDisplaySurroundingDamage();
    }

    public void TryDisplaySurroundingDamage()
    {
        if (State == CellState.Hidden)
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
}

public enum CellState { Hidden, Revealed, Interacted, Cleared }