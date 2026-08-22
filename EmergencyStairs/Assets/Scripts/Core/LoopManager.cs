using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// P.T.のような「同じ廊下を繰り返す」構造を管理するステートマシン。
/// ループ回数と進行フラグを一元管理し、LoopVariantやトリガー等が購読して見た目・音を切り替える。
/// </summary>
public class LoopManager : MonoSingleton<LoopManager>
{
    [SerializeField] private UnityEvent<int> onLoopAdvancedEvent;

    public int CurrentLoop { get; private set; } = 0;
    public event Action<int> OnLoopAdvanced;

    private readonly HashSet<string> flags = new();

    public void AdvanceLoop()
    {
        CurrentLoop++;
        OnLoopAdvanced?.Invoke(CurrentLoop);
        onLoopAdvancedEvent?.Invoke(CurrentLoop);
    }

    public void ResetLoops()
    {
        CurrentLoop = 0;
        flags.Clear();
        OnLoopAdvanced?.Invoke(CurrentLoop);
        onLoopAdvancedEvent?.Invoke(CurrentLoop);
    }

    /// <summary>デバッグ/検証用に任意のループへ直接ジャンプする。</summary>
    public void SetLoop(int loop)
    {
        CurrentLoop = loop;
        OnLoopAdvanced?.Invoke(CurrentLoop);
        onLoopAdvancedEvent?.Invoke(CurrentLoop);
    }

    public void SetFlag(string key) => flags.Add(key);
    public void ClearFlag(string key) => flags.Remove(key);
    public bool HasFlag(string key) => flags.Contains(key);
}
