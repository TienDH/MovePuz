using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    private static string sceneToLoad;

    public static void LoadScene(string sceneName)
    {
        sceneToLoad = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    private void Start()
    {
        StartCoroutine(LoadAsync());
    }

    private System.Collections.IEnumerator LoadAsync()
    {
        yield return new WaitForSeconds(0.3f); // Cho UI hiện lên mượt

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneToLoad);
        operation.allowSceneActivation = false;

        while (!operation.isDone)
        {
            float progress = Mathf.Clamp01(operation.progress / 0.9f);
            progressBar.fillAmount = progress;

            if (progress >= 1f)
            {
                yield return new WaitForSeconds(1.5f);
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
