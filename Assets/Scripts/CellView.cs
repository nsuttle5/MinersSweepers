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
    public bool revealed = false;

    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private Sprite revealedSprite;

    [SerializeField] private TextMeshPro markText;
    [SerializeField] private TextMeshPro damageText;

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
            if (revealed) return;

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

    public void Reveal()
    {
        if (revealed) return;
        revealed = true;
        UpdateVisual();

        if (spawnable != null)
        {
            LogbookManager.Instance.Discover(spawnable);
        }

        if (boardManager != null && spawnable == null)
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

        if (!revealed)
        {
            if (hiddenSprite != null)
            {
                sr.sprite = hiddenSprite;
            }

            sr.color = Color.white;

            if (damageText) damageText.gameObject.SetActive(false);
        }
        else
        {
            if (spawnable != null && spawnable.sprite != null)
            {
                sr.sprite = spawnable.sprite;
            }
            else if (revealedSprite != null)
            {
                sr.sprite = revealedSprite;
            }

            sr.color = Color.white;

            if (damageText) damageText.gameObject.SetActive(false);
        }
    }
}