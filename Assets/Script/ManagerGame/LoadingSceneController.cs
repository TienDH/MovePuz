using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class LoadingSceneController : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private FadePanel fadePanel;

    static string _targetScene;
    static bool _isBusy;

    [Header("Tuning")]
    [SerializeField] float minShowSeconds = 1.0f;   // thời gian hiển thị tối thiểu
    [SerializeField] float smoothTime = 0.25f;  // độ mượt thanh bar
    [SerializeField] float fadeOutTime = 0.25f;  // thời gian kéo bar 0.95→1 khi mở màn

    public static void LoadScene(string sceneName)
    {
        if (_isBusy) return;
        _isBusy = true;
        _targetScene = sceneName;
        SceneManager.LoadScene("LoadingScene", LoadSceneMode.Single);
    }

    private void OnDisable() { _isBusy = false; }

    private IEnumerator Start()
    {
        Time.timeScale = 1f;
        if (string.IsNullOrEmpty(_targetScene)) yield break;

        // Che ngay nếu có
        if (fadePanel) fadePanel.SetInstant(1f, true);
        yield return null;

        var op = SceneManager.LoadSceneAsync(_targetScene, LoadSceneMode.Single);
        op.allowSceneActivation = false;

        float displayed = 0f;        // giá trị hiển thị
        float vel = 0f;               // tốc độ cho SmoothDamp
        float t = 0f;

        // cập nhật cho đến khi đã nạp xong (op.progress≈0.9) và đủ thời gian tối thiểu
        while (true)
        {
            t += Time.unscaledDeltaTime;

            // mục tiêu hiển thị: khóa ở 0.95 để dành 0.05 cho giai đoạn mở màn
            float target = Mathf.Clamp01((op.progress / 0.9f) * 0.95f);

            displayed = Mathf.SmoothDamp(displayed, target, ref vel, smoothTime, Mathf.Infinity, Time.unscaledDeltaTime);
            if (progressBar) progressBar.fillAmount = displayed;

            bool loaded = op.progress >= 0.9f;
            bool timeOk = t >= minShowSeconds;

            if (loaded && timeOk) break; // đã sẵn sàng activate + đủ thời gian hiển thị
            yield return null;
        }

        // Kéo bar 0.95 → 1.0 trong lúc fade-out
        bool fadeDone = false;
        if (fadePanel) fadePanel.FadeOut(() => fadeDone = true, fadeOutTime);

        float endTimer = 0f;
        while (displayed < 1f || !fadeDone)
        {
            endTimer += Time.unscaledDeltaTime;
            displayed = Mathf.MoveTowards(displayed, 1f, Time.unscaledDeltaTime / fadeOutTime);
            if (progressBar) progressBar.fillAmount = displayed;
            yield return null;
        }

        op.allowSceneActivation = true;
        while (!op.isDone) yield return null;
    }
}
