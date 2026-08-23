using System;
using R3;
using UnityEngine;
using UnityEngine.UI;

public enum VolumeTarget
{
    Bgm,
    Se
}

/// <summary>
/// SliderをAudioManagerの指定音量(BGM/SE)へ双方向で結びつけるだけの単機能コンポーネント。
/// Sliderにこれを付けてtargetを選ぶだけで、タイトル/ポーズなどどの設定画面でも同じ挙動になる
/// (TitleMenuControllerのような画面側のコントローラーはスライダーの存在を一切知らなくてよい)。
/// </summary>
[RequireComponent(typeof(Slider))]
public class AudioVolumeSliderBinding : MonoBehaviour
{
    [SerializeField] private VolumeTarget target = VolumeTarget.Bgm;

    private Slider slider;
    private IDisposable subscription;

    private void Awake()
    {
        slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        if (AudioManager.Instance == null) return;

        ReactiveProperty<float> property = target == VolumeTarget.Bgm
            ? AudioManager.Instance.BgmVolume
            : AudioManager.Instance.SeVolume;

        slider.SetValueWithoutNotify(property.Value);
        subscription = slider.OnValueChangedAsObservable().Subscribe(value => Apply(value));
    }

    private void OnDisable()
    {
        subscription?.Dispose();
        subscription = null;
    }

    private void Apply(float value)
    {
        if (target == VolumeTarget.Bgm)
            AudioManager.Instance.SetBGMVolume(value);
        else
            AudioManager.Instance.SetSEVolume(value);
    }
}
