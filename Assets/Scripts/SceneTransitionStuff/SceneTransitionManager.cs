using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;

    [SerializeField] private GameObject clickBlocker;
    [SerializeField] private List<BaseTransitionEffect> transitionEffects;
    [SerializeField] private TransitionType defaultType = TransitionType.ShaderHorizontalWipe;

    public static UnityAction<string, LoadSceneMode, bool?> OnTransitionSceneLoaded;

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

    public void LoadScene(string sceneName, TransitionType? type = null, LoadSceneMode mode = LoadSceneMode.Single, bool? needsHUD = null) =>
        StartCoroutine(MasterTransitionRoutine(new(sceneName), type ?? defaultType, mode, needsHUD));

    public void LoadScene(int sceneNum, TransitionType? type = null, LoadSceneMode mode = LoadSceneMode.Single, bool? needsHUD = null) =>
        StartCoroutine(MasterTransitionRoutine(new(sceneNum), type ?? defaultType, mode, needsHUD));

    private IEnumerator MasterTransitionRoutine(SceneTarget sceneTarget, TransitionType selectedType, LoadSceneMode mode, bool? needsHUD)
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

        AsyncOperation op = sceneTarget.IsIndex
            ? SceneManager.LoadSceneAsync(sceneTarget.BuildIndex, mode)
            : SceneManager.LoadSceneAsync(sceneTarget.Name, mode);

        while (!op.isDone) yield return null;

        string loadedSceneName = sceneTarget.IsIndex 
            ? SceneManager.GetSceneByBuildIndex(sceneTarget.BuildIndex).name : sceneTarget.Name;

        OnTransitionSceneLoaded?.Invoke(loadedSceneName, mode, needsHUD);

        yield return StartCoroutine(activeEffect.TransitionOut());
        clickBlocker.SetActive(false);
    }

    private struct SceneTarget
    {
        public string Name;
        public int BuildIndex;
        public readonly bool IsIndex => BuildIndex >= 0;

        public SceneTarget(string name) { Name = name; BuildIndex = -1; }
        public SceneTarget(int index) { Name = ""; BuildIndex = index; }
    }
}

public enum TransitionType
{
    ShaderHorizontalWipe = 0,
}