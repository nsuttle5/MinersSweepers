using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MapViewController : MonoBehaviour
{
    [Header("Animation View Settings")]
    [SerializeField] private float fullViewShowDuration = 2.0f;
    [SerializeField] private float transitionSpeed = 3.5f;

    [Header("Manual Offset")]
    [SerializeField] private float panRightManualOffset = 0f;

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
        contentRt.pivot = new Vector2(0f, 0.5f);
    }

    private void Start()
    {
        StartCoroutine(ExecuteIntroSequence());
    }

    private IEnumerator ExecuteIntroSequence()
    {
        yield return null;

        yield return new WaitUntil(() => MapManager.Instance != null);

        var currentNode = MapManager.Instance.currentNode;
        Transform targetNodeTransform = (currentNode != null && currentNode.buttonUI != null) ? currentNode.buttonUI.transform : null;

        if (targetNodeTransform == null)
        {
            for (int i = 0; i < contentRt.childCount; i++)
            {
                if (contentRt.GetChild(i).TryGetComponent<MapNodeButton>(out var btn))
                {
                    targetNodeTransform = btn.transform;
                    currentNode ??= btn.nodeData;
                    break;
                }
            }
        }

        float contentWidth = contentRt.rect.width;
        float viewportWidth = viewportRt.rect.width;

        if (MapManager.Instance.CurrentLevel == 0)
        {
            if (contentWidth > viewportWidth && viewportWidth > 0)
            {
                float fitScaleFactor = viewportWidth / contentWidth;
                initialFullViewScale = new Vector3(fitScaleFactor, fitScaleFactor, 1f);
            }
            else
            {
                initialFullViewScale = Vector3.one;
            }

            contentRt.localScale = initialFullViewScale;
            scrollRect.horizontalNormalizedPosition = 0f;

            if (targetNodeTransform != null)
            {
                CalculateFocusCenter(targetNodeTransform.GetComponent<RectTransform>().anchoredPosition, Vector3.one, 0f);
            }

            yield return new WaitForSeconds(fullViewShowDuration);
            isAnimatingTransition = true;
        }
        else
        {
            initialFullViewScale = Vector3.one;
            contentRt.localScale = Vector3.one;
            targetScale = Vector3.one;

            Transform previousNodeTransform = null;

            for (int i = 0; i < contentRt.childCount; i++)
            {
                var btn = contentRt.GetChild(i).GetComponent<MapNodeButton>();
                if (btn != null && btn.nodeData != null && btn.nodeData.levelIndex == currentNode.levelIndex - 1)
                {
                    previousNodeTransform = btn.transform;
                    break;
                }
            }

            if (previousNodeTransform != null)
            {
                CalculateFocusCenter(previousNodeTransform.GetComponent<RectTransform>().anchoredPosition, Vector3.one, panRightManualOffset);
                scrollRect.horizontalNormalizedPosition = targetHorizontalNormalizedPos;
            }
            else
            {
                scrollRect.horizontalNormalizedPosition = 0f;
            }

            if (targetNodeTransform != null)
            {
                CalculateFocusCenter(targetNodeTransform.GetComponent<RectTransform>().anchoredPosition, Vector3.one, panRightManualOffset);
            }

            yield return new WaitForSeconds(fullViewShowDuration);
            isAnimatingTransition = true;
        }
    }

    private void CalculateFocusCenter(Vector2 nodeLocalPosition, Vector3 referenceScale, float customOffsetPixels)
    {
        float contentWidth = contentRt.rect.width;
        float viewportWidth = viewportRt.rect.width;

        float currentScaleX = referenceScale.x;
        float scaledContentWidth = contentWidth * currentScaleX;

        if (scaledContentWidth <= viewportWidth)
        {
            targetHorizontalNormalizedPos = 0f;
            return;
        }

        float localNodeXScaled = nodeLocalPosition.x * currentScaleX;
        float targetCenterScrollPixel = localNodeXScaled - (viewportWidth * 0.5f) + customOffsetPixels;
        float maxScrollablePixelDistance = scaledContentWidth - viewportWidth;
        float clampedScrollPixel = Mathf.Clamp(targetCenterScrollPixel, 0f, maxScrollablePixelDistance);
        targetHorizontalNormalizedPos = clampedScrollPixel / maxScrollablePixelDistance;
    }

    private void LateUpdate()
    {
        if (!isAnimatingTransition) return;

        float delta = Time.deltaTime * transitionSpeed;

        contentRt.localScale = Vector3.Lerp(contentRt.localScale, targetScale, delta);

        float currentScrollPos = scrollRect.horizontalNormalizedPosition;
        float nextScrollPos = Mathf.Lerp(currentScrollPos, targetHorizontalNormalizedPos, delta);
        scrollRect.horizontalNormalizedPosition = nextScrollPos;

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
