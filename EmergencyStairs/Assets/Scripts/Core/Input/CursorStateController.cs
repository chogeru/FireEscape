using System;
using R3;
using UnityEngine;

/// <summary>
/// GameInputManager.GameplayInputEnabledを購読し、カーソルのロック/表示状態に反映するだけの
/// 単機能コンポーネント。カーソル制御をGameInputManager本体から切り離すことで、
/// 「入力状態の集計」と「その結果どう振る舞うか」を疎結合にする。
/// </summary>
public class CursorStateController : MonoBehaviour
{
    private IDisposable subscription;

    private void OnEnable()
    {
        subscription = GameInputManager.Instance.GameplayInputEnabled.Subscribe(enabled => Apply(enabled));
    }

    private void OnDisable()
    {
        subscription?.Dispose();
        subscription = null;
    }

    private void Apply(bool gameplayInputEnabled)
    {
        Cursor.lockState = gameplayInputEnabled ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !gameplayInputEnabled;
    }
}
