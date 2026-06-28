using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MapViewController : MonoBehaviour
{
    [Header("Animation View Settings")]
    [Tooltip("How long the player views the entire map at start before zoom-focus transitions.")]
    [SerializeField] private float fullViewShowDuration = 2.0f;
    [Tooltip("Speed multiplier for the panning motion and zoom scaling transitions.")]
    [SerializeField] private float transitionSpeed = 3.5f;

    private ScrollRect scrollRect;
    private RectTransform viewportRt;
    private RectTransform contentRt;

    private float targetHorizontalNormalizedPos;
    private Vector3 targetScale = Vector3.one;
    private Vector3 initialFullViewScale = Vector3.one;

    private bool isAnimatingTransition = false;

    private void Awake()
    {
        scrollRect = GetComponent<ScrollRect>();
        viewportRt = scrollRect.viewport != null ? scrollRect.viewport : GetComponent<RectTransform>();
        contentRt = scrollRect.content;

        // Force Left-Center Pivot configuration on Content panel to secure scaling calculations completely
        contentRt.pivot = new Vector2(0f, 0.5f);
    }

    private void Start()
    {
        StartCoroutine(ExecuteIntroSequencePipeline());
    }

    private IEnumerator ExecuteIntroSequencePipeline()
    {
        // Wait until MapManager instance initializes completely inside scene memory
        yield return new WaitUntil(() => MapManager.Instance != null);

        Transform targetNodeTransform = null;

        // Route camera directly to the player's active progression node coordinates
        if (MapManager.Instance.currentNode != null && MapManager.Instance.currentNode.buttonUI != null)
        {
            targetNodeTransform = MapManager.Instance.currentNode.buttonUI.transform;
        }
        else
        {
            // Scan structural elements to find Level 0 node if active progress is null
            for (int i = 0; i < contentRt.childCount; i++)
            {
                if (contentRt.GetChild(i).GetComponent<MapNodeButton>() != null)
                {
                    targetNodeTransform = contentRt.GetChild(i);
                    break;
                }
            }
        }

        float contentWidth = contentRt.rect.width;
        float viewportWidth = viewportRt.rect.width;

        // VERIFIED UNIFORM ZOOM FIX: Scale X and Y by the same ratio to zoom out completely without warping art
        if (contentWidth > viewportWidth && viewportWidth > 0)
        {
            float fitScaleFactor = viewportWidth / contentWidth;
            initialFullViewScale = new Vector3(fitScaleFactor, fitScaleFactor, 1f);
        }
        else
        {
            initialFullViewScale = Vector3.one;
        }

        // Apply proportional zoomed-out scale immediately on stage launch
        contentRt.localScale = initialFullViewScale;
        scrollRect.horizontalNormalizedPosition = 0f;

        // Pre-compute Phase 2 centering parameters while player is looking over the landscape overview
        if (targetNodeTransform != null)
        {
            CalculateFocusCenteringNormalization(targetNodeTransform.GetComponent<RectTransform>().anchoredPosition);
        }

        yield return new WaitForSeconds(fullViewShowDuration);

        // Turn on real-time animation transitions inside late update cycles
        isAnimatingTransition = true;
    }

    private void CalculateFocusCenteringNormalization(Vector2 nodeLocalPosition)
    {
        float contentWidth = contentRt.rect.width;
        float viewportWidth = viewportRt.rect.width;

        if (contentWidth <= viewportWidth)
        {
            targetHorizontalNormalizedPos = 0f;
            return;
        }

        // True Midpoint Centering Rule: Places targeted node pixel coordinates exactly in the screen center
        float desiredScrollPixelX = nodeLocalPosition.x - (viewportWidth * 0.5f);
        float maxScrollableDistance = contentWidth - viewportWidth;

        // Clamp boundaries safely so scrolling maps never drift away or introduce empty workspace gaps
        float clampedScrollPixelX = Mathf.Clamp(desiredScrollPixelX, 0f, maxScrollableDistance);

        targetHorizontalNormalizedPos = clampedScrollPixelX / maxScrollableDistance;
        targetScale = Vector3.one; // Focus target returns smoothly back to full native uniform scale (1.0)
    }

    private void LateUpdate()
    {
        if (!isAnimatingTransition) return;

        float delta = Time.deltaTime * transitionSpeed;

        // Smoothly return both X and Y scales back to uniform native 1.0 specifications
        contentRt.localScale = Vector3.Lerp(contentRt.localScale, targetScale, delta);

        // Smoothly center the active level node onto the viewport display midpoint grid lines
        float currentScrollPos = scrollRect.horizontalNormalizedPosition;
        float nextScrollPos = Mathf.Lerp(currentScrollPos, targetHorizontalNormalizedPos, delta);
        scrollRect.horizontalNormalizedPosition = nextScrollPos;

        // Evaluation termination criteria check to release execution trackers smoothly
        bool scaleRestored = Vector3.Distance(contentRt.localScale, targetScale) < 0.002f;
        bool positionCentered = Mathf.Abs(nextScrollPos - targetHorizontalNormalizedPos) < 0.001f;

        if (scaleRestored && positionCentered)
        {
            contentRt.localScale = targetScale;
            scrollRect.horizontalNormalizedPosition = targetHorizontalNormalizedPos;
            isAnimatingTransition = false;
        }
    }
}
