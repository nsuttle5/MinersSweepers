using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [SerializeField] private GameObject clickBlocker;
    [SerializeField] private List<BaseTransitionEffect> transitionEffects;
    [SerializeField] private TransitionType defaultType = TransitionType.ShaderHorizontalWipe;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        clickBlocker.SetActive(false);
    }

    public void LoadScene(string sceneName, TransitionType? type = null) =>
        StartCoroutine(MasterTransitionRoutine(sceneName, -1, type ?? defaultType));

    public void LoadScene(int sceneNum, TransitionType? type = null) =>
        StartCoroutine(MasterTransitionRoutine("", sceneNum, type ?? defaultType));

    private IEnumerator MasterTransitionRoutine(string sceneName, int sceneNum, TransitionType selectedType)
    {
        int effectIndex = (int)selectedType;
        if (effectIndex >= transitionEffects.Count || transitionEffects[effectIndex] == null)
        {
            Debug.LogError("The transition effect you chose isnt here");
            yield break;
        }

        clickBlocker.SetActive(true);
        BaseTransitionEffect activeEffect = transitionEffects[effectIndex];
        yield return StartCoroutine(activeEffect.TransitionIn());

        AsyncOperation op;
        if (!string.IsNullOrEmpty(sceneName)) op = SceneManager.LoadSceneAsync(sceneName);
        else op = SceneManager.LoadSceneAsync(sceneNum);

        while (!op.isDone) yield return null;

        yield return StartCoroutine(activeEffect.TransitionOut());
        clickBlocker.SetActive(false);
    }
}

public enum TransitionType
{
    ShaderHorizontalWipe = 0,
}