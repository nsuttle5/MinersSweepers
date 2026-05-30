using UnityEngine;
using UnityEngine.EventSystems;

public class CellContextMenu : MonoBehaviour
{
    [SerializeField] private RectTransform menuRect;
    [SerializeField] private GameObject clickBlocker;

    private float menuHeight;
    private float menuWidth;
    private Camera mainCam;
    private CellView currentCell;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    private void OnEnable()
    {
        CellView.OnCellRightClick += OpenMenu;
    }

    private void OnDisable()
    {
        CellView.OnCellRightClick -= OpenMenu;
    }

    private void Start()
    {
        menuHeight = menuRect.rect.height;
        menuWidth = menuRect.rect.width;

        CloseMenu();
    }

    private void OpenMenu(CellView cell, Vector2 cellWorldPos)
    {
        currentCell = cell;
        menuRect.gameObject.SetActive(true);
        clickBlocker.SetActive(true);

        Canvas.ForceUpdateCanvases();
        menuHeight = menuRect.rect.height;
        menuWidth = menuRect.rect.width;

        float cellWorldWidth = 1f;
        if (cell.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
        {
            cellWorldWidth = spriteRenderer.bounds.size.x;
        }
        else if (cell.TryGetComponent<BoxCollider2D>(out var boxCollider))
        {
            cellWorldWidth = boxCollider.size.x * cell.transform.lossyScale.x;
        }

        Vector2 cellCenterScreen = mainCam.WorldToScreenPoint(cellWorldPos);
        Vector2 cellRightEdgeWorld = cellWorldPos + new Vector2(cellWorldWidth / 2f, 0f);
        Vector2 cellRightEdgeScreen = mainCam.WorldToScreenPoint(cellRightEdgeWorld);

        float halfCellPixelWidth = cellRightEdgeScreen.x - cellCenterScreen.x;

        Vector2 targetPos = cellCenterScreen;
        targetPos.x = cellCenterScreen.x + halfCellPixelWidth + (menuWidth / 2f);

        if (targetPos.x + (menuWidth / 2f) > Screen.width)
        {
            targetPos.x = cellCenterScreen.x - halfCellPixelWidth - (menuWidth / 2f);
        }
        else if (targetPos.x - (menuWidth / 2f) < 0)
        {
            targetPos.x = menuWidth / 2f;
        }

        if (targetPos.y + (menuHeight / 2f) > Screen.height)
        {
            targetPos.y = Screen.height - (menuHeight / 2f);
        }
        else if (targetPos.y - (menuHeight / 2f) < 0)
        {
            targetPos.y = menuHeight / 2f;
        }

        menuRect.position = targetPos;
    }

    public void SelectMarking(string symbol)
    {
        if (currentCell != null)
        {
            currentCell.Mark(symbol);
        }

        CloseMenu();
    }

    public void CloseMenu()
    {
        currentCell = null;
        menuRect.gameObject.SetActive(false);
        clickBlocker.SetActive(false);
    }
}
