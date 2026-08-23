using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>AudioManagerのうち、3D SEプールの障害物オクルージョン(Sounds Good style)を担当する部分。</summary>
public partial class AudioManager
{
    [FoldoutGroup("Occlusion (Sounds Good style)"), Tooltip("壁やドアなど障害物越しの3D SEを自動でこもらせる。")]
    [SerializeField] private bool enableOcclusion = true;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private LayerMask occlusionLayers = ~0;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionMinCutoff = 500f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionMaxCutoff = 22000f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField, Range(0f, 1f)] private float occlusionMinVolumeMultiplier = 0.35f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionCheckInterval = 0.15f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionLerpSpeed = 6f;

    private readonly List<AudioLowPassFilter> sePoolLowPass = new();
    private readonly List<float> sePoolOcclusionFactor = new();
    private readonly List<float> sePoolOcclusionLerp = new();
    private readonly List<float> sePoolOcclusionTimer = new();

    private Transform listenerTransform;

    /// <summary>SEプール初期化時に、オクルージョン用のフィルタと状態配列を1スロット分追加する。</summary>
    private void InitializeOcclusionForSlot(GameObject go)
    {
        var lpf = go.AddComponent<AudioLowPassFilter>();
        lpf.cutoffFrequency = occlusionMaxCutoff;
        lpf.enabled = enableOcclusion;
        sePoolLowPass.Add(lpf);
        sePoolOcclusionFactor.Add(0f);
        sePoolOcclusionLerp.Add(0f);
        sePoolOcclusionTimer.Add(0f);
    }

    /// <summary>SE再生開始時に、そのスロットのオクルージョン状態をリセットする。</summary>
    private void ResetOcclusionForSlot(int index, bool is3D)
    {
        sePoolOcclusionFactor[index] = 0f;
        sePoolOcclusionLerp[index] = 0f;
        sePoolOcclusionTimer[index] = 0f;
        sePoolLowPass[index].enabled = enableOcclusion && is3D;
        sePoolLowPass[index].cutoffFrequency = occlusionMaxCutoff;
    }

    private void UpdateOcclusion(int index)
    {
        if (!sePoolLowPass[index].enabled) return;
        if (!TryGetListenerTransform(out var listener)) return;

        sePoolOcclusionTimer[index] -= Time.deltaTime;
        if (sePoolOcclusionTimer[index] <= 0f)
        {
            sePoolOcclusionTimer[index] = occlusionCheckInterval;

            Vector3 origin = listener.position;
            Vector3 target = sePool[index].transform.position;
            Vector3 dir = target - origin;
            float distance = dir.magnitude;

            bool blocked = distance > 0.1f &&
                Physics.Raycast(origin, dir.normalized, distance, occlusionLayers, QueryTriggerInteraction.Ignore);

            sePoolOcclusionFactor[index] = blocked ? 1f : 0f;
        }

        float lerped = Mathf.MoveTowards(sePoolOcclusionLerp[index], sePoolOcclusionFactor[index],
            Time.deltaTime * occlusionLerpSpeed);
        sePoolOcclusionLerp[index] = lerped;

        float cutoff = Mathf.Lerp(occlusionMaxCutoff, occlusionMinCutoff, lerped);
        float volumeMul = Mathf.Lerp(1f, occlusionMinVolumeMultiplier, lerped);

        sePoolLowPass[index].cutoffFrequency = cutoff;
        sePool[index].volume = sePoolBaseVolume[index] * volumeMul;
    }

    private bool TryGetListenerTransform(out Transform listener)
    {
        if (listenerTransform == null)
        {
            var found = FindFirstObjectByType<AudioListener>();
            listenerTransform = found != null ? found.transform : null;
        }
        listener = listenerTransform;
        return listener != null;
    }
}
