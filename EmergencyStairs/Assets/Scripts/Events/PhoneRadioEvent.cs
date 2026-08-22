using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private string audioSeId;
    [SerializeField] private List<SubtitleLine> subtitles = new();
    [SerializeField] private bool attachToSelf = true;
    [Tooltip("ONにすると再生中カーソルを表示しゲームプレイ入力を止める(GameInputManager経由)。歩きながら聞かせたい場合はOFF")]
    [SerializeField] private bool requestUIMode = false;
    [SerializeField] private UnityEvent onEventStarted;
    [SerializeField] private UnityEvent onEventFinished;

    private Coroutine playRoutine;
    private AudioSource activeSource;

    public void Play()
    {
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            if (activeSource != null)
                activeSource.Stop();
        }
        playRoutine = StartCoroutine(PlayRoutine());
    }

    public void Stop()
    {
        if (playRoutine != null)
            StopCoroutine(playRoutine);
        playRoutine = null;

        if (activeSource != null)
            activeSource.Stop();
        activeSource = null;

        if (requestUIMode)
            GameInputManager.Instance?.ReleaseUIMode(this);

        SubtitleUI.Instance?.Hide();
    }

    private IEnumerator PlayRoutine()
    {
        if (AudioManager.Instance == null)
            yield break;

        if (requestUIMode)
            GameInputManager.Instance?.RequestUIMode(this);

        onEventStarted?.Invoke();

        activeSource = attachToSelf
            ? AudioManager.Instance.PlaySEAttached(audioSeId, transform)
            : AudioManager.Instance.PlaySEAtPoint(audioSeId, transform.position);

        float clipLength = AudioManager.Instance.GetSEClipLength(audioSeId);
        float elapsed = 0f;
        int index = 0;

        while (elapsed < clipLength)
        {
            while (index < subtitles.Count && elapsed >= subtitles[index].startTime)
            {
                SubtitleUI.Instance?.Show(subtitles[index].text);
                index++;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        SubtitleUI.Instance?.Hide();

        if (requestUIMode)
            GameInputManager.Instance?.ReleaseUIMode(this);

        onEventFinished?.Invoke();
        playRoutine = null;
    }
}
