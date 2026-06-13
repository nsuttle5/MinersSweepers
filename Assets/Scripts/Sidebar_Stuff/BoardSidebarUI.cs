using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class BoardSidebarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private RectTransform tabButton;
    [SerializeField] private Transform slotParent;
    [SerializeField] private GameObject slotPrefab;

    [Header("Slide Settings")]
    [SerializeField] private float slideInX = 0f;
    [SerializeField] private float slideOutX = 300f;
    [SerializeField] private float slideDuration = 0.3f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private bool _isOpen = false;
    private bool _isAnimating = false;
    private List<GameObject> _slots = new();
    private Coroutine _slideCoroutine;

    private void OnEnable()
    {
        BoardSidebarTracker.OnTrackerUpdated += RefreshUI;
    }

    private void OnDisable()
    {
        BoardSidebarTracker.OnTrackerUpdated -= RefreshUI;
    }

    private void Start()
    {
        Vector2 pos = panel.anchoredPosition;
        pos.x = slideOutX;
        panel.anchoredPosition = pos;
    }

    public void TogglePanel()
    {
        if (_isAnimating) return;
        _isOpen = !_isOpen;

        if (_slideCoroutine != null) StopCoroutine(_slideCoroutine);
        _slideCoroutine = StartCoroutine(SlidePanel(_isOpen ? slideInX : slideOutX));
    }

    private IEnumerator SlidePanel(float targetX)
    {
        _isAnimating = true;
        float startX = panel.anchoredPosition.x;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
            Vector2 pos = panel.anchoredPosition;
            pos.x = Mathf.Lerp(startX, targetX, t);
            panel.anchoredPosition = pos;
            yield return null;
        }

        Vector2 finalPos = panel.anchoredPosition;
        finalPos.x = targetX;
        panel.anchoredPosition = finalPos;
        _isAnimating = false;
    }

    private void RefreshUI()
    {
        foreach (var slot in _slots)
            Destroy(slot);
        _slots.Clear();

        if (BoardSidebarTracker.Instance == null) return;

        List<BoardSidebarEntry> entries = BoardSidebarTracker.Instance.GetEntries();
        foreach (var entry in entries)
        {
            GameObject go = Instantiate(slotPrefab, slotParent);
            go.GetComponent<BoardSidebarSlotUI>().Setup(entry);
            _slots.Add(go);
        }
    }
}