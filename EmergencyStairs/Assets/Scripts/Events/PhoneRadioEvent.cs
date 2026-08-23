using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SubtitleLine
{
    [TextArea] public string text;
    [Tooltip("音声再生開始からの経過秒数でこの字幕を表示")]
    public float startTime;
}

/// <summary>
/// 電話・ラジオなど「音声+字幕」のタイミング制御。
/// AudioManagerのPlaySEAttached/PlaySEAtPointで3D音声を鳴らしつつ、
/// 指定した経過時間ごとにSubtitleUIへ字幕を出す。TriggerEventZoneのUnityEventからPlay()を呼ぶ想定。
/// </summary>
public class PhoneRadioEvent : MonoBehaviour
{
    [SerializeField, ValueDropdown(nameof(GetSeIdOptions))] private string audioSeId;
    [SerializeField] private List<SubtitleLine> subtitles = new();

#if UNITY_EDITOR
    private static IEnumerable<string> GetSeIdOptions() => SoundLibraryIds.SeIds();
#else
    private static IEnumerable<string> GetSeIdOptions() => System.Linq.Enumerable.Empty<string>();
#endif
    [SerializeField] private bool attachToSelf = true;
    [Tooltip("ONにすると再生中カーソルを表示しゲームプレイ入力を止める(GameInputManager経由)。歩きながら聞かせたい場合はOFF")]
    [SerializeField] private bool requestUIMode = false;
    [SerializeField] private UnityEvent onEventStarted;
    [SerializeField] private UnityEvent onEventFinished;

    private CancellationTokenSource playCts;
    private AudioSource activeSource;

    public void Play()
    {
        playCts?.Cancel();
        playCts?.Dispose();
        playCts = new CancellationTokenSource();
        PlayAsync(playCts.Token).Forget();
    }

    public void Stop()
    {
        playCts?.Cancel();
        playCts?.Dispose();
        playCts = null;

        if (activeSource != null)
            activeSource.Stop();
        activeSource = null;

        if (requestUIMode)
            GameInputManager.Instance?.ReleaseUIMode(this);

        SubtitleUI.Instance?.Hide();
    }

    private async UniTaskVoid PlayAsync(CancellationToken ct)
    {
        if (AudioManager.Instance == null) return;

        if (requestUIMode)
            GameInputManager.Instance?.RequestUIMode(this);

        onEventStarted?.Invoke();

        activeSource = attachToSelf
            ? AudioManager.Instance.PlaySEAttached(audioSeId, transform)
            : AudioManager.Instance.PlaySEAtPoint(audioSeId, transform.position);

        float clipLength = AudioManager.Instance.GetSEClipLength(audioSeId);
        float elapsed = 0f;
        int index = 0;

        try
        {
            while (elapsed < clipLength)
            {
                while (index < subtitles.Count && elapsed >= subtitles[index].startTime)
                {
                    SubtitleUI.Instance?.Show(subtitles[index].text);
                    index++;
                }

                elapsed += Time.deltaTime;
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        }
        catch (OperationCanceledException)
        {
            // Stop()側で後始末済みなので、ここでは何もせず終了する
            return;
        }

        SubtitleUI.Instance?.Hide();

        if (requestUIMode)
            GameInputManager.Instance?.ReleaseUIMode(this);

        onEventFinished?.Invoke();
    }

    private void OnDestroy()
    {
        playCts?.Cancel();
        playCts?.Dispose();
    }
}
