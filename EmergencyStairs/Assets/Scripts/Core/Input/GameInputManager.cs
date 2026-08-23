using System.Collections.Generic;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 「今ゲームプレイ入力を受け付けてよいか」を一元管理する参照カウント式の状態サービス。
/// ポーズメニュー・電話/ラジオイベント・将来のダイアログUIなど、複数のシステムが
/// 同時にUIモードを要求しても RequestUIMode/ReleaseUIMode の参照カウントで正しく解決される
/// (どれか1つでもUIモードを要求していればゲームプレイ入力はOFFのまま)。
/// 状態の集計のみに責任を絞っており、カーソル制御などの副作用はCursorStateControllerが
/// GameplayInputEnabledを購読して個別に担当する(疎結合)。
/// </summary>
public class GameInputManager : MonoSingleton<GameInputManager>
{
    private readonly HashSet<object> uiModeRequesters = new();

    public ReactiveProperty<bool> GameplayInputEnabled { get; } = new(true);

    public bool IsGameplayInputEnabled => GameplayInputEnabled.Value;

    [ShowInInspector, ReadOnly] private bool GameplayEnabledDebug => IsGameplayInputEnabled;
    [ShowInInspector, ReadOnly] private int UiModeRequesterCountDebug => uiModeRequesters.Count;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GameplayInputEnabled.Dispose();
    }

    /// <summary>UIモードを要求する。requesterは要求元自身(this)を渡し、Release時に同じ参照を渡すこと。</summary>
    public void RequestUIMode(object requester)
    {
        uiModeRequesters.Add(requester);
        Refresh();
    }

    public void ReleaseUIMode(object requester)
    {
        uiModeRequesters.Remove(requester);
        Refresh();
    }

    private void Refresh()
    {
        bool enabled = uiModeRequesters.Count == 0;
        if (GameplayInputEnabled.Value == enabled) return;

        GameplayInputEnabled.Value = enabled;
    }
}
