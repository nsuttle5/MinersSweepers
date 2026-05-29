using UnityEngine;
using TMPro;

public class CellView : MonoBehaviour
{
    public int x, y;
    public SpawnableSO spawnable;
    public BoardManager boardManager;
    private SpriteRenderer sr;
    public bool revealed = false;

    public TextMeshPro damageText;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (!damageText)
            damageText = GetComponentInChildren<TextMeshPro>();
        if (damageText)
            damageText.gameObject.SetActive(false);
    }

    private void OnMouseDown()
    {
        if (!revealed && boardManager != null)
        {
            boardManager.OnCellClicked(x, y);
        }
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