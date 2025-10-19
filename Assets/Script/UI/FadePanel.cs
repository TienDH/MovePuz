using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class FadePanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 1f;

    Coroutine _co;

    void Awake()
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    /// Đặt alpha ngay lập tức (không chạy coroutine)
    public void SetInstant(float alpha, bool blockInput = false)
    {
        if (_co != null) { StopCoroutine(_co); _co = null; }
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = Mathf.Clamp01(alpha);
        canvasGroup.blocksRaycasts = blockInput;
        canvasGroup.interactable = blockInput;
    }

    public void FadeIn(Action onComplete = null, float? duration = null)
    {
        // FadeIn = che màn (alpha 0 -> 1), chặn input trong lúc che
        StartFade(0f, 1f, true, onComplete, duration ?? fadeDuration);
    }

    public void FadeOut(Action onComplete = null, float? duration = null)
    {
        // FadeOut = mở màn (alpha 1 -> 0), bỏ chặn input khi xong
        StartFade(1f, 0f, false, onComplete, duration ?? fadeDuration);
    }

    void StartFade(float startAlpha, float endAlpha, bool blockDuring, Action onComplete, float duration)
    {
        if (_co != null) StopCoroutine(_co);
        _co = StartCoroutine(Fade(startAlpha, endAlpha, blockDuring, onComplete, duration));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha, bool blockDuring, Action onComplete, float duration)
    {
        if (!canvasGroup) yield break;

        canvasGroup.blocksRaycasts = blockDuring;
        canvasGroup.interactable = blockDuring;

        float t = 0f;
        canvasGroup.alpha = startAlpha;

        if (duration <= 0f)
        {
            canvasGroup.alpha = endAlpha;
        }
        else
        {
            while (t < 1f)
            {
                t += Time.unscaledDeltaTime / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
                yield return null;
            }
            canvasGroup.alpha = endAlpha;
        }

        if (Mathf.Approximately(endAlpha, 0f))
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        onComplete?.Invoke();
        _co = null;
    }
}
