using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 電話/ラジオイベント用の簡易字幕表示。CanvasGroupのフェードで表示/非表示する。
/// シーンにCanvas + Text + CanvasGroupを用意し、このコンポーネントを付けて参照を割り当てる。
/// </summary>
public class SubtitleUI : MonoSingleton<SubtitleUI>
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Text label;
    [SerializeField] private float fadeSpeed = 8f;

    private Coroutine fadeRoutine;

    protected override void Awake()
    {
        base.Awake();
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
    }

    public void Show(string text)
    {
        if (label != null)
            label.text = text;
        SetVisible(true);
    }

    public void Hide()
    {
        SetVisible(false);
    }

    private void SetVisible(bool visible)
    {
        if (canvasGroup == null) return;
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeTo(visible ? 1f : 0f));
    }

    private IEnumerator FadeTo(float target)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, target))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, target, fadeSpeed * Time.deltaTime);
            yield return null;
        }
    }
}
