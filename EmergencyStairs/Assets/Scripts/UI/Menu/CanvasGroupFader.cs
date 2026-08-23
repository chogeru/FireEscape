using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// CanvasGroup.alphaをDOTweenで補間するIScreenFader実装。黒幕Image+CanvasGroupに付けて使う汎用モジュール。
/// </summary>
public class CanvasGroupFader : MonoBehaviour, IScreenFader
{
    [SerializeField, Required] private CanvasGroup canvasGroup;
    [SerializeField] private float duration = 1f;
    [SerializeField] private Ease ease = Ease.InOutSine;

    private Tween activeTween;

    private void Reset()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnDestroy()
    {
        activeTween?.Kill();
    }

    public async UniTask FadeAsync(float from, float to, CancellationToken ct = default)
    {
        if (canvasGroup == null) return;

        activeTween?.Kill();
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = from;

        if (duration <= 0f)
        {
            canvasGroup.alpha = to;
        }
        else
        {
            activeTween = canvasGroup.DOFade(to, duration).SetEase(ease).SetUpdate(true);
            await activeTween.ToUniTask(cancellationToken: ct);
        }

        canvasGroup.blocksRaycasts = to > 0.99f;
    }
}
