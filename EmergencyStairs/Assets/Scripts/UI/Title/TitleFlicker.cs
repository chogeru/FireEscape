using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 非常灯のような不規則な明滅を対象のGraphicに与える、タイトル画面用の演出コンポーネント。
/// DOTweenで次の明滅先へフェードし、完了コールバックで次の間隔・目標alphaを再抽選し続ける。
/// </summary>
public class TitleFlicker : MonoBehaviour
{
    [SerializeField, Required] private Graphic target;
    [SerializeField] private float minAlpha = 0.55f;
    [SerializeField] private float maxAlpha = 1f;
    [SerializeField] private float minInterval = 0.03f;
    [SerializeField] private float maxInterval = 0.6f;
    [Tooltip("この秒数が経過するまでは明滅させず、通常表示を保つ")]
    [SerializeField] private float steadyDuration = 2f;

    private Tween flickerTween;

    private void Reset()
    {
        target = GetComponent<Graphic>();
    }

    private void OnEnable()
    {
        if (target == null) target = GetComponent<Graphic>();
        flickerTween?.Kill();
        flickerTween = DOVirtual.DelayedCall(steadyDuration, PlayNextFlicker, false);
    }

    private void OnDisable()
    {
        flickerTween?.Kill();
    }

    private void PlayNextFlicker()
    {
        float targetAlpha = Random.Range(minAlpha, maxAlpha);
        float interval = Random.Range(minInterval, maxInterval);
        flickerTween = target.DOFade(targetAlpha, interval).SetEase(Ease.Linear).OnComplete(PlayNextFlicker);
    }
}
