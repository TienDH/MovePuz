using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneChanger : MonoBehaviour
{
    public void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }
    public float delayTime = 0.2f; // Thời gian delay trước khi chuyển scene

    public void ChangeScene(string sceneName)
    {
        StartCoroutine(DelayChangeScene(sceneName));
    }

    

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(DelayLoadLevel(sceneName));
    }

    public void ResetCurrentScene()
    {
        StartCoroutine(DelayResetCurrentScene());
    }

    private IEnumerator DelayChangeScene(string sceneName)
    {
        yield return new WaitForSecondsRealtime(delayTime); // Không bị ảnh hưởng bởi Time.timeScale
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator DelayLoadLevel(string sceneName)
    {
        yield return new WaitForSecondsRealtime(delayTime);
        LoadingSceneController.LoadScene(sceneName);
    }

    private IEnumerator DelayResetCurrentScene()
    {
        yield return new WaitForSecondsRealtime(delayTime);
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
