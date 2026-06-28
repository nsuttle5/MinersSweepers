using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ScrollRect))]
public class MapViewController : MonoBehaviour
{
    [Header("Animation View Settings")]
    [SerializeField] private float fullViewShowDuration = 2.0f;
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
        contentRt.pivot = new Vector2(0f, 0.5f);
    }

    private void Start()
    {
        StartCoroutine(ExecuteIntroSequencePipeline());
    }

    private IEnumerator ExecuteIntroSequencePipeline()
    {
        yield return new WaitUntil(() => MapManager.Instance != null);

        Transform targetNodeTransform = null;

        if (MapManager.Instance.currentNode != null && MapManager.Instance.currentNode.buttonUI != null)
        {
            targetNodeTransform = MapManager.Instance.currentNode.buttonUI.transform;
        }
        else
        {
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
            CalculateFocusCenteringNormalization(targetNodeTransform.GetComponent<RectTransform>().anchoredPosition);
        }

        yield return new WaitForSeconds(fullViewShowDuration);

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

        float desiredScrollPixelX = nodeLocalPosition.x - (viewportWidth * 0.5f);
        float maxScrollableDistance = contentWidth - viewportWidth;

        float clampedScrollPixelX = Mathf.Clamp(desiredScrollPixelX, 0f, maxScrollableDistance);

        targetHorizontalNormalizedPos = clampedScrollPixelX / maxScrollableDistance;
        targetScale = Vector3.one;
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
