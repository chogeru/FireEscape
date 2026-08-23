using R3;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Time.timeScaleによるポーズ管理。GameInputManagerにUIモードを要求するだけで
/// カーソル表示・ゲームプレイ入力停止が自動的に連動する(仕組みはPhoneRadioEvent等と共通)。
/// 他システムはIsPausedRP(ReactiveProperty)を購読するだけで、ポーズ中の挙動を自由に実装できる。
/// 購読した瞬間に現在値を受け取れるので、後から生成されたUI等も状態の取りこぼしがない。
/// </summary>
public class PauseManager : MonoSingleton<PauseManager>
{
    public ReactiveProperty<bool> IsPausedRP { get; } = new(false);

    public bool IsPaused => IsPausedRP.Value;

    [ShowInInspector, ReadOnly] private bool IsPausedDebug => IsPaused;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        IsPausedRP.Dispose();
    }

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

        Time.timeScale = paused ? 0f : 1f;

        if (paused)
            GameInputManager.Instance.RequestUIMode(this);
        else
            GameInputManager.Instance.ReleaseUIMode(this);

        IsPausedRP.Value = paused;
    }
}
