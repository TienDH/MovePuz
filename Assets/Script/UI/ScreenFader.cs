using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class ScreenFader : MonoBehaviour
{
    [SerializeField] private Animator transitionAnim;
    [SerializeField] private float transitionTime = 1f;   // bằng chiều dài clip fade-out
    [SerializeField] private string startTrigger = "Start";
    [SerializeField] private string endTrigger = "End";

    // Khi vào scene mới, mở sáng (nếu Animator không auto)

    // Gọi hàm này để chuyển scene theo tên
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {

        //transitionAnim.SetTrigger(endTrigger);
        transitionAnim.SetTrigger(startTrigger);// fade out
        yield return new WaitForSecondsRealtime(transitionTime);
        SceneManager.LoadScene(sceneName);
        // load theo tên
    }
}
