using System.Collections.Generic;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// P.T.のような「同じ廊下を繰り返す」構造を管理するステートマシン。
/// ループ回数と進行フラグを一元管理し、LoopVariantやトリガー等が購読して見た目・音を切り替える。
/// CurrentLoopはReactivePropertyなので、購読した瞬間に現在のループ数を受け取れる
/// (後から生成されたオブジェクトも、直前までの進行状況を取りこぼさない)。
/// </summary>
public class LoopManager : MonoSingleton<LoopManager>
{
    [Tooltip("Inspector上での配線用(ドラッグ&ドロップで既存コンポーネントのメソッドを繋げる)")]
    [SerializeField] private UnityEvent<int> onLoopAdvancedEvent;

    public ReactiveProperty<int> CurrentLoopRP { get; } = new(0);

    public int CurrentLoop => CurrentLoopRP.Value;

    [ShowInInspector, ReadOnly] private int CurrentLoopDebug => CurrentLoop;
    [ShowInInspector, ReadOnly] private List<string> FlagsDebug => new(flags);

    private readonly HashSet<string> flags = new();

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CurrentLoopRP.Dispose();
    }

    [Button("Advance Loop"), PropertyOrder(-1)]
    public void AdvanceLoop() => SetLoop(CurrentLoop + 1);

    [Button("Reset Loops"), PropertyOrder(-1)]
    public void ResetLoops()
    {
        flags.Clear();
        SetLoop(0);
    }

    /// <summary>デバッグ/検証用に任意のループへ直接ジャンプする。</summary>
    public void SetLoop(int loop)
    {
        CurrentLoopRP.Value = loop;
        onLoopAdvancedEvent?.Invoke(loop);
    }

    public void SetFlag(string key) => flags.Add(key);
    public void ClearFlag(string key) => flags.Remove(key);
    public bool HasFlag(string key) => flags.Contains(key);
}
