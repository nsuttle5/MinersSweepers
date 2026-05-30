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

    [SerializeField] private TextMeshPro markText;
    [SerializeField] private TextMeshPro damageText;

    public static UnityAction<CellView, Vector2> OnCellRightClick;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!damageText) Debug.LogError("DamageText not set on CellView prefab");
        if (!markText) Debug.LogError("MarkText not set on CellView prefab");
        
        if (damageText) damageText.gameObject.SetActive(false);
        if (markText) markText.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
        {
            if (!revealed && boardManager != null)
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
        if (!revealed)
        {
            sr.color = Color.gray;
            // sr.sprite = hiddenSprite;  //when we have a covered-tile sprite

            if (damageText) damageText.gameObject.SetActive(false);
        }
        else if (spawnable == null)
        {
            sr.color = Color.white;

        }
        else
        {
            sr.color = Color.white;
            if (spawnable.sprite != null)
            {
                sr.sprite = spawnable.sprite;
            }

            if (damageText) damageText.gameObject.SetActive(false);
        }
    }
}