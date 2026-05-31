using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ShaderTransitionEffect : BaseTransitionEffect
{
    [SerializeField] private float transitionDuration = 0.25f;

    public override float Duration => transitionDuration;

    private Material matInstance;
    private readonly int progressProperty = Shader.PropertyToID("_Progress");

    private void Awake()
    {
        Image img = GetComponent<Image>();
        matInstance = new Material(img.material);
        img.material = matInstance;

        matInstance.SetFloat(progressProperty, 1f);
    }

    public override IEnumerator TransitionIn()
    {
        float timer = 0;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(-1f, 0f, timer / transitionDuration);
            matInstance.SetFloat(progressProperty, progress);
            yield return null;
        }
        matInstance.SetFloat(progressProperty, 0f);
    }

    public override IEnumerator TransitionOut()
    {
        float timer = 0;
        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = Mathf.Lerp(0f, 1f, timer / transitionDuration);
            matInstance.SetFloat(progressProperty, progress);
            yield return null;
        }
        matInstance.SetFloat(progressProperty, 1f);
    }
}
