using UnityEngine;
using UnityEngine.UI;

public class MinorManager : MonoBehaviour
{
    [SerializeField] private Button swapButton;
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite firstSprite;
    [SerializeField] private Sprite secondSprite;

    private bool showingFirstSprite = true;

    private void Awake()
    {
        ApplyCurrentSprite();
    }

    private void OnEnable()
    {
        if (swapButton != null)
        {
            swapButton.onClick.AddListener(SwapSprite);
        }
    }

    private void Start()
    {
        ApplyCurrentSprite();
    }

    private void OnDisable()
    {
        if (swapButton != null)
        {
            swapButton.onClick.RemoveListener(SwapSprite);
        }
    }

    public void SwapSprite()
    {
        showingFirstSprite = !showingFirstSprite;
        ApplyCurrentSprite();
    }

    public void SetFirstSprite()
    {
        showingFirstSprite = true;
        ApplyCurrentSprite();
    }

    public void SetSecondSprite()
    {
        showingFirstSprite = false;
        ApplyCurrentSprite();
    }

    private void ApplyCurrentSprite()
    {
        if (targetImage == null)
        {
            return;
        }

        Sprite nextSprite = showingFirstSprite ? firstSprite : secondSprite;
        if (nextSprite != null)
        {
            targetImage.sprite = nextSprite;
        }
    }
}
