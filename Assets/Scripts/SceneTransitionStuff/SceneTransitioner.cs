using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitioner : MonoBehaviour
{
    [SerializeField] private TransitionType transitionType;
    
    public void LoadScene(int numberIndex)
    {
        if (SceneTransitionManager.Instance) SceneTransitionManager.Instance.LoadScene(numberIndex, transitionType);
        else SceneManager.LoadScene(numberIndex);
    }

    public void LoadScene(string sceneName)
    {
        if (SceneTransitionManager.Instance) SceneTransitionManager.Instance.LoadScene(sceneName, transitionType);
        else SceneManager.LoadScene(sceneName);
    }
}
