using System.Collections;
using UnityEngine;

public abstract class BaseTransitionEffect : MonoBehaviour
{
    public abstract float Duration { get; }
    public abstract IEnumerator TransitionIn();
    public abstract IEnumerator TransitionOut();
}
