using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CellContextMenu : MonoBehaviour
{
    [Header("Menu Transforms")]
    [SerializeField] private RectTransform menuRect;
    [SerializeField] private GameObject clickBlocker;

    [Header("Operation Toggles")]
    [SerializeField] private Toggle addToggleElement;
    [SerializeField] private Toggle subToggleElement;
    [SerializeField] private Toggle setToToggleElement;

    [Header("Action Triggers")]
    [SerializeField] private Button clearOverrideButton;
    [SerializeField] private Button mineButtonElement;

    private float menuHeight;
    private float menuWidth;
    private Camera mainCam;
    private CellView currentCell;

    public ConfigModMode SelectedMode { get; private set; } = ConfigModMode.SetTo;

    private void Awake()
    {
        mainCam = Camera.main;

        if (addToggleElement) addToggleElement.onValueChanged.AddListener((isOn) => { if (isOn) SyncModSelection(ConfigModMode.Add); });
        if (subToggleElement) subToggleElement.onValueChanged.AddListener((isOn) => { if (isOn) SyncModSelection(ConfigModMode.Subtract); });
        if (setToToggleElement) setToToggleElement.onValueChanged.AddListener((isOn) => { if (isOn) SyncModSelection(ConfigModMode.SetTo); });

        if (clearOverrideButton) clearOverrideButton.onClick.AddListener(ProcessClearIntent);
        if (mineButtonElement) mineButtonElement.onClick.AddListener(PressMineMarkingButton);
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

        if (setToToggleElement) setToToggleElement.isOn = true;

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

    private void SyncModSelection(ConfigModMode chosenMode)
    {
        SelectedMode = chosenMode;
        if (addToggleElement) addToggleElement.SetIsOnWithoutNotify(chosenMode == ConfigModMode.Add);
        if (subToggleElement) subToggleElement.SetIsOnWithoutNotify(chosenMode == ConfigModMode.Subtract);
        if (setToToggleElement) setToToggleElement.SetIsOnWithoutNotify(chosenMode == ConfigModMode.SetTo);
    }

    public void PressNumericDigitButton(int numericValueMark)
    {
        if (currentCell == null) return;

        if (SelectedMode != ConfigModMode.SetTo && int.TryParse(currentCell.MarkText, out int number))
        {
            if (SelectedMode == ConfigModMode.Add) numericValueMark = number + numericValueMark;
            else numericValueMark = number - numericValueMark;

            numericValueMark = Mathf.Max(0, numericValueMark);
        }

        string customTextSymbol = (numericValueMark == 0) ? "" : numericValueMark.ToString();

        currentCell.Mark(customTextSymbol);

        FinalizeGridRecalculationPass();
        CloseMenu();
    }

    public void PressMineMarkingButton()
    {
        if (currentCell == null) return;
        currentCell.Mark("*");

        FinalizeGridRecalculationPass();
        CloseMenu();
    }

    private void ProcessClearIntent()
    {
        if (currentCell == null) return;

        currentCell.Mark(string.Empty);
        FinalizeGridRecalculationPass();
        CloseMenu();
    }

    private void FinalizeGridRecalculationPass()
    {
        if (currentCell == null) return;

        currentCell.UpdateVisual();
        currentCell.TryDisplaySurroundingDamage();
        if (currentCell.boardManager)
        {
            currentCell.boardManager.RefreshAllCellVisuals();
            currentCell.boardManager.RefreshAllCellDamageValues();
        }
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

public enum ConfigModMode { Add, Subtract, SetTo }