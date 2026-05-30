using System.Collections;
using UnityEngine;

public class AnimatorTransitionEffect : BaseTransitionEffect
{
    [SerializeField] private Animator animator;
    [SerializeField] private float transitionDuration = 0.5f;
    [SerializeField] private string inTrigger = "StartFade";
    [SerializeField] private string outTrigger = "EndFade";

    public override float Duration => transitionDuration;

    public override IEnumerator TransitionIn()
    {
        if (animator) animator.SetTrigger(inTrigger);
        yield return new WaitForSeconds(transitionDuration);
    }

    public override IEnumerator TransitionOut()
    {
        if (animator) animator.SetTrigger(outTrigger);
        yield return new WaitForSeconds(transitionDuration);
    }
}
