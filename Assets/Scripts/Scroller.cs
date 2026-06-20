using UnityEngine;
using UnityEngine.UI;

public class Scroller : MonoBehaviour
{
    [SerializeField] private RawImage rawImage;
    [SerializeField] private float scrollSpeedX = 1f;
    [SerializeField] private float scrollSpeedY = 1f;

    // Update is called once per frame
    void Update()
    {
        rawImage.uvRect = new Rect(rawImage.uvRect.position + new Vector2(scrollSpeedX, scrollSpeedY) * Time.deltaTime, rawImage.uvRect.size);

    }
}
