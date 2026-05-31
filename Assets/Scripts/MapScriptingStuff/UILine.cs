using UnityEngine;
using UnityEngine.UI;

public class UILine : MonoBehaviour
{
    [Space]
    [SerializeField] Image image;

    public void SetLine(Vector2 start, Vector2 end, float cutDist, float thickness, Color color)
    {
        if (image == null)
            image = GetComponent<Image>();

        Vector2 dir = (end - start).normalized;
        Vector2 trim = dir * cutDist;
        Vector2 newStart = start + trim;
        Vector2 newEnd = end - trim;
        Vector2 diff = newEnd - newStart;
        float length = diff.magnitude;

        var rt = GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(length, thickness);
        rt.pivot = new Vector2(0, 0.5f);
        rt.anchoredPosition = newStart;
        float angle = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        if (image != null)
            image.color = color;
    }
}