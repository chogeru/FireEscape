using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 「今ゲームプレイ入力を受け付けてよいか」を一元管理するイベント駆動ハブ。
/// ポーズメニュー・電話/ラジオイベント・将来のダイアログUIなど、複数のシステムが
/// 同時にUIモードを要求しても RequestUIMode/ReleaseUIMode の参照カウントで正しく解決される
/// (どれか1つでもUIモードを要求していればゲームプレイ入力はOFFのまま)。
/// カーソルのロック/解除もここで一元制御し、各コンポーネントはCursor.lockStateを直接触らない。
/// </summary>
public class GameInputManager : MonoSingleton<GameInputManager>
{
    private readonly HashSet<object> uiModeRequesters = new();

    public bool IsGameplayInputEnabled => uiModeRequesters.Count == 0;
    public event Action<bool> OnGameplayInputEnabledChanged;

    protected override void Awake()
    {
        base.Awake();
        ApplyCursorState();
    }

    /// <summary>UIモードを要求する。requesterは要求元自身(this)を渡し、Release時に同じ参照を渡すこと。</summary>
    public void RequestUIMode(object requester)
    {
        bool wasEnabled = IsGameplayInputEnabled;
        uiModeRequesters.Add(requester);
        if (wasEnabled != IsGameplayInputEnabled)
            NotifyChanged();
    }

    public void ReleaseUIMode(object requester)
    {
        bool wasEnabled = IsGameplayInputEnabled;
        uiModeRequesters.Remove(requester);
        if (wasEnabled != IsGameplayInputEnabled)
            NotifyChanged();
    }

    private void NotifyChanged()
    {
        ApplyCursorState();
        OnGameplayInputEnabledChanged?.Invoke(IsGameplayInputEnabled);
    }

    private void ApplyCursorState()
    {
        bool gameplay = IsGameplayInputEnabled;
        Cursor.lockState = gameplay ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !gameplay;
    }
}
