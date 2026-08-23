using System.Collections;
using System.Collections.Generic;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>AudioManagerのうち、3D SEプールの再生・管理・SE音量を担当する部分。</summary>
public partial class AudioManager
{
    [FoldoutGroup("Sources"), SerializeField] private AudioSource seSource2D;

    [FoldoutGroup("3D SE Pool"), SerializeField] private int sePoolSize = 8;

    [FoldoutGroup("Volume"), SerializeField, Range(0f, 1f)] private float seVolume = 1f;

    /// <summary>現在のSE音量(0-1)。用途はBgmVolumeと同様。</summary>
    public ReactiveProperty<float> SeVolume { get; } = new(1f);

    [FoldoutGroup("Volume"), ShowInInspector, ReadOnly, ProgressBar(0, 1), LabelText("Se Volume (live)")]
    private float SeVolumeDisplay => SeVolume.Value;

    private const string SeVolumePrefKey = "AudioManager.SeVolume";

    private readonly List<AudioSource> sePool = new();
    private readonly List<Transform> sePoolFollowTarget = new();
    private readonly List<int> sePoolToken = new();
    private readonly List<float> sePoolBaseVolume = new();

    private void InitializeSeVolume()
    {
        if (PlayerPrefs.HasKey(SeVolumePrefKey)) seVolume = PlayerPrefs.GetFloat(SeVolumePrefKey);
        SeVolume.Value = seVolume;
    }

    private void InitializeSePool()
    {
        if (seSource2D == null)
        {
            seSource2D = gameObject.AddComponent<AudioSource>();
        }
        seSource2D.loop = false;
        seSource2D.playOnAwake = false;
        seSource2D.spatialBlend = 0f;

        for (int i = 0; i < sePoolSize; i++)
        {
            var go = new GameObject($"SE3D_{i}");
            go.transform.SetParent(transform);
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            sePool.Add(src);
            sePoolFollowTarget.Add(null);
            sePoolToken.Add(0);
            sePoolBaseVolume.Add(0f);

            InitializeOcclusionForSlot(go);
        }
    }

    private void UpdateSePool()
    {
        for (int i = 0; i < sePool.Count; i++)
        {
            if (sePoolFollowTarget[i] != null)
                sePool[i].transform.position = sePoolFollowTarget[i].position;

            if (enableOcclusion && sePool[i].isPlaying)
                UpdateOcclusion(i);
        }
    }

    /// <summary>定位のない2D SE(UI音など)。SoundEntry.is3Dの値に関わらず2D再生。</summary>
    public void PlaySE(string id, float volumeScale = 1f)
    {
        if (!TryGetSE(id, out var entry)) return;

        seSource2D.pitch = entry.pitch;
        seSource2D.PlayOneShot(entry.clip, entry.volume * seVolume * volumeScale);
    }

    /// <summary>指定位置で立体音響再生。足音・環境音・敵の鳴き声など。戻り値は呼び出し元で明示的に止めたい場合用。</summary>
    public AudioSource PlaySEAtPoint(string id, Vector3 position, float volumeScale = 1f)
    {
        var src = PrepareSource(id, volumeScale, out var entry, out int index, out int token);
        if (src == null) return null;

        src.transform.position = position;
        sePoolFollowTarget[index] = null;
        src.Play();
        StartCoroutine(ReleaseAfter(index, token, entry.clip.length / Mathf.Max(entry.pitch, 0.01f)));
        return src;
    }

    /// <summary>指定Transformに追従しながら立体音響再生(移動する発生源向け)。戻り値は呼び出し元で明示的に止めたい場合用。</summary>
    public AudioSource PlaySEAttached(string id, Transform followTarget, float volumeScale = 1f)
    {
        var src = PrepareSource(id, volumeScale, out var entry, out int index, out int token);
        if (src == null) return null;

        src.transform.position = followTarget.position;
        sePoolFollowTarget[index] = followTarget;
        src.Play();
        StartCoroutine(ReleaseAfter(index, token, entry.clip.length / Mathf.Max(entry.pitch, 0.01f)));
        return src;
    }

    private AudioSource PrepareSource(string id, float volumeScale, out SoundEntry entry, out int index, out int token)
    {
        index = -1;
        token = 0;

        if (!TryGetSE(id, out entry)) return null;

        index = GetAvailableIndex();
        if (index < 0) return null;

        var src = sePool[index];
        float baseVolume = entry.volume * seVolume * volumeScale;
        src.clip = entry.clip;
        src.volume = baseVolume;
        src.pitch = entry.pitch;
        src.spatialBlend = entry.is3D ? entry.spatialBlend : 0f;
        src.minDistance = entry.minDistance;
        src.maxDistance = entry.maxDistance;
        src.rolloffMode = entry.rolloffMode;

        sePoolBaseVolume[index] = baseVolume;
        ResetOcclusionForSlot(index, entry.is3D);

        token = ++sePoolToken[index];
        return src;
    }

    /// <summary>電話/ラジオイベントの字幕タイミング計算などに使う、登録済みSEクリップの長さ(秒)。</summary>
    public float GetSEClipLength(string id)
    {
        if (library != null && library.TryGetSE(id, out var entry) && entry.clip != null)
            return entry.clip.length;
        return 0f;
    }

    private int GetAvailableIndex()
    {
        for (int i = 0; i < sePool.Count; i++)
        {
            if (!sePool[i].isPlaying)
                return i;
        }

        if (sePool.Count == 0) return -1;

        Debug.LogWarning("[AudioManager] SE pool exhausted, reusing oldest source.");
        return 0;
    }

    private IEnumerator ReleaseAfter(int index, int token, float delay)
    {
        yield return new WaitForSeconds(Mathf.Max(delay, 0.05f));
        // tokenが変わっていたら、この間に同じスロットが別のSEに使い回された(=このコルーチンは無効)
        if (sePoolToken[index] == token)
            sePoolFollowTarget[index] = null;
    }

    private bool TryGetSE(string id, out SoundEntry entry)
    {
        entry = null;
        if (library == null || !library.TryGetSE(id, out entry))
        {
            Debug.LogWarning($"[AudioManager] SE not found: {id}");
            return false;
        }
        return true;
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        SeVolume.Value = seVolume;
        PlayerPrefs.SetFloat(SeVolumePrefKey, seVolume);
        PlayerPrefs.Save();
    }
}
