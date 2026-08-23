using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// 恐怖度(FearLevel)に応じて心拍・呼吸音の音量/ピッチを滑らかに変化させる。
/// 追跡AIやジャンプスケアなど他システムからSetFearLevel/AddFearを呼んで駆動する想定。
/// AudioManagerのプールとは独立した専用ループAudioSource(2D)をプレイヤーに直付けする。
/// </summary>
public class HeartbeatAmbience : MonoBehaviour
{
    [FoldoutGroup("Sources"), SerializeField] private AudioSource heartbeatSource;
    [FoldoutGroup("Sources"), SerializeField] private AudioSource breathingSource;

    [FoldoutGroup("Curves"), SerializeField] private AnimationCurve heartbeatVolumeByFear = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [FoldoutGroup("Curves"), SerializeField] private AnimationCurve heartbeatPitchByFear = AnimationCurve.Linear(0f, 0.9f, 1f, 1.4f);
    [FoldoutGroup("Curves"), SerializeField] private AnimationCurve breathingVolumeByFear = AnimationCurve.Linear(0f, 0.1f, 1f, 0.8f);

    [SerializeField] private float smoothTime = 1.5f;

    private float targetFear;
    private float currentFear;
    private float fearVelocity;

    public float FearLevel => currentFear;

    [ShowInInspector, ReadOnly, ProgressBar(0, 1)]
    private float FearLevelDebug => currentFear;

    public void SetFearLevel(float value) => targetFear = Mathf.Clamp01(value);
    public void AddFear(float delta) => targetFear = Mathf.Clamp01(targetFear + delta);

    [Button("Fear -> Max"), PropertyOrder(-1)]
    private void DebugSetFearMax() => SetFearLevel(1f);

    [Button("Fear -> Zero"), PropertyOrder(-1)]
    private void DebugSetFearZero() => SetFearLevel(0f);

    private void Awake()
    {
        if (heartbeatSource != null)
        {
            heartbeatSource.loop = true;
            heartbeatSource.spatialBlend = 0f;
        }
        if (breathingSource != null)
        {
            breathingSource.loop = true;
            breathingSource.spatialBlend = 0f;
        }
    }

    private void Update()
    {
        currentFear = Mathf.SmoothDamp(currentFear, targetFear, ref fearVelocity, smoothTime);

        if (heartbeatSource != null)
        {
            heartbeatSource.volume = heartbeatVolumeByFear.Evaluate(currentFear);
            heartbeatSource.pitch = heartbeatPitchByFear.Evaluate(currentFear);
            if (!heartbeatSource.isPlaying && heartbeatSource.clip != null)
                heartbeatSource.Play();
        }

        if (breathingSource != null)
        {
            breathingSource.volume = breathingVolumeByFear.Evaluate(currentFear);
            if (!breathingSource.isPlaying && breathingSource.clip != null)
                breathingSource.Play();
        }
    }
}
