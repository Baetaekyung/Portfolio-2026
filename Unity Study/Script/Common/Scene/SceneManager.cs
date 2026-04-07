using UnityEngine;

public enum ESceneName
{
    Patch,
    Title,
    InGame,
}

[SingletonFlag(ESingletonFlag.DONT_DESTROY)]
public class SceneManager : Singleton<SceneManager>
{
    public void LoadSceneSync(ESceneName sceneName)
    {
        var sceneNameToString = sceneName.ToString();
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNameToString);
    }

    public AsyncOperation LoadSceneAsync(ESceneName sceneName)
    {
        var sceneNameToString = sceneName.ToString();
        return UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(sceneNameToString);
    }
}
