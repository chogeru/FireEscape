using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Time.timeScaleによるポーズ管理。GameInputManagerにUIモードを要求するだけで
/// カーソル表示・ゲームプレイ入力停止が自動的に連動する(仕組みはPhoneRadioEvent等と共通)。
/// 他システムはOnPauseChangedを購読するだけで、ポーズ中の挙動を自由に実装できる。
/// </summary>
public class PauseManager : MonoSingleton<PauseManager>
{
    public bool IsPaused { get; private set; }
    public event Action<bool> OnPauseChanged;

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            return;

        // 自分以外(電話イベント等)がUIモードを要求中は、ポーズ開閉で割り込まない
        if (!IsPaused && GameInputManager.Instance != null && !GameInputManager.Instance.IsGameplayInputEnabled)
            return;

        TogglePause();
    }

    public void TogglePause() => SetPaused(!IsPaused);

    public void SetPaused(bool paused)
    {
        if (IsPaused == paused) return;
        IsPaused = paused;

        Time.timeScale = paused ? 0f : 1f;

        if (paused)
            GameInputManager.Instance.RequestUIMode(this);
        else
            GameInputManager.Instance.ReleaseUIMode(this);

        OnPauseChanged?.Invoke(paused);
    }
}
