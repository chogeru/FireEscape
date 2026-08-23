using System;
using R3;
using UnityEngine;

/// <summary>
/// 同じ部屋にある小道具の「差し替え」を表現する汎用コンポーネント。
/// 対象GameObjectに付け、ループ回数の範囲を指定するだけで自動的に表示/非表示が切り替わる。
/// 例: Photo_Normal(minLoop=0,maxLoop=1) / Photo_Broken(minLoop=2,maxLoop=999) を同じ場所に重ねて置く。
/// </summary>
public class LoopVariant : MonoBehaviour
{
    [SerializeField] private int minLoop = 0;
    [SerializeField] private int maxLoop = int.MaxValue;

    private IDisposable loopSubscription;

    private void OnEnable()
    {
        if (LoopManager.Instance != null)
            loopSubscription = LoopManager.Instance.CurrentLoopRP.Subscribe(loop => Refresh(loop));
    }

    private void OnDisable()
    {
        loopSubscription?.Dispose();
        loopSubscription = null;
    }

    private void Refresh(int loop)
    {
        gameObject.SetActive(loop >= minLoop && loop <= maxLoop);
    }
}
