using System.Collections;
using System.Collections.Generic;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

public class AudioManager : MonoSingleton<AudioManager>
{
    [FoldoutGroup("Sound Data"), SerializeField, Required] private SoundLibrary library;
    [FoldoutGroup("Sound Data"), Tooltip("設定するとStart()で自動的にBGM再生を開始する(未設定なら何もしない)。")]
    [SerializeField] private string startBgmId;
    [FoldoutGroup("Sound Data"), SerializeField] private float startBgmFadeTime = 2f;

    [FoldoutGroup("Sources"), SerializeField] private AudioSource bgmSource;
    [FoldoutGroup("Sources"), SerializeField] private AudioSource seSource2D;

    [FoldoutGroup("3D SE Pool"), SerializeField] private int sePoolSize = 8;

    [FoldoutGroup("Volume"), SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;
    [FoldoutGroup("Volume"), SerializeField, Range(0f, 1f)] private float seVolume = 1f;

    [FoldoutGroup("Occlusion (Sounds Good style)"), Tooltip("壁やドアなど障害物越しの3D SEを自動でこもらせる。")]
    [SerializeField] private bool enableOcclusion = true;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private LayerMask occlusionLayers = ~0;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionMinCutoff = 500f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionMaxCutoff = 22000f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField, Range(0f, 1f)] private float occlusionMinVolumeMultiplier = 0.35f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionCheckInterval = 0.15f;
    [FoldoutGroup("Occlusion (Sounds Good style)"), SerializeField] private float occlusionLerpSpeed = 6f;

    /// <summary>現在のBGM音量(0-1)。値の変更を購読すれば、オプション画面のスライダー等をリアクティブに同期できる。</summary>
    public ReactiveProperty<float> BgmVolume { get; } = new(1f);

    /// <summary>現在のSE音量(0-1)。用途はBgmVolumeと同様。</summary>
    public ReactiveProperty<float> SeVolume { get; } = new(1f);

    [FoldoutGroup("Volume"), ShowInInspector, ReadOnly, ProgressBar(0, 1), LabelText("Bgm Volume (live)")]
    private float BgmVolumeDisplay => BgmVolume.Value;

    [FoldoutGroup("Volume"), ShowInInspector, ReadOnly, ProgressBar(0, 1), LabelText("Se Volume (live)")]
    private float SeVolumeDisplay => SeVolume.Value;

    private string currentBgmId;
    private Coroutine fadeRoutine;

    private readonly List<AudioSource> sePool = new();
    private readonly List<Transform> sePoolFollowTarget = new();
    private readonly List<int> sePoolToken = new();
    private readonly List<AudioLowPassFilter> sePoolLowPass = new();
    private readonly List<float> sePoolBaseVolume = new();
    private readonly List<float> sePoolOcclusionFactor = new();
    private readonly List<float> sePoolOcclusionLerp = new();
    private readonly List<float> sePoolOcclusionTimer = new();

    private Transform listenerTransform;

    protected override void Awake()
    {
        base.Awake();

        if (PlayerPrefs.HasKey(BgmVolumePrefKey)) bgmVolume = PlayerPrefs.GetFloat(BgmVolumePrefKey);
        if (PlayerPrefs.HasKey(SeVolumePrefKey)) seVolume = PlayerPrefs.GetFloat(SeVolumePrefKey);
        BgmVolume.Value = bgmVolume;
        SeVolume.Value = seVolume;

        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;

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

            var lpf = go.AddComponent<AudioLowPassFilter>();
            lpf.cutoffFrequency = occlusionMaxCutoff;
            lpf.enabled = enableOcclusion;
            sePoolLowPass.Add(lpf);
            sePoolBaseVolume.Add(0f);
            sePoolOcclusionFactor.Add(0f);
            sePoolOcclusionLerp.Add(0f);
            sePoolOcclusionTimer.Add(0f);
        }
    }

    private void Start()
    {
        if (!string.IsNullOrEmpty(startBgmId))
            PlayBGM(startBgmId, true, startBgmFadeTime);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BgmVolume.Dispose();
        SeVolume.Dispose();
    }

    private void LateUpdate()
    {
        for (int i = 0; i < sePool.Count; i++)
        {
            if (sePoolFollowTarget[i] != null)
                sePool[i].transform.position = sePoolFollowTarget[i].position;

            if (enableOcclusion && sePool[i].isPlaying)
                UpdateOcclusion(i);
        }
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

    public void PlayBGM(string id, bool loop = true, float fadeTime = 0f)
    {
        if (library == null || !library.TryGetBGM(id, out var entry))
        {
            Debug.LogWarning($"[AudioManager] BGM not found: {id}");
            return;
        }

        if (currentBgmId == id && bgmSource.isPlaying)
            return;

        currentBgmId = id;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (fadeTime > 0f)
        {
            fadeRoutine = StartCoroutine(FadeToNewBGM(entry, loop, fadeTime));
        }
        else
        {
            bgmSource.clip = entry.clip;
            bgmSource.volume = entry.volume * bgmVolume;
            bgmSource.pitch = entry.pitch;
            bgmSource.loop = loop;
            bgmSource.Play();
        }
    }

    public void StopBGM(float fadeTime = 0f)
    {
        currentBgmId = null;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        if (fadeTime > 0f)
            fadeRoutine = StartCoroutine(FadeOutAndStop(fadeTime));
        else
            bgmSource.Stop();
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
        sePoolOcclusionFactor[index] = 0f;
        sePoolOcclusionLerp[index] = 0f;
        sePoolOcclusionTimer[index] = 0f;
        sePoolLowPass[index].enabled = enableOcclusion && entry.is3D;
        sePoolLowPass[index].cutoffFrequency = occlusionMaxCutoff;

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

    private const string BgmVolumePrefKey = "AudioManager.BgmVolume";
    private const string SeVolumePrefKey = "AudioManager.SeVolume";

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
        BgmVolume.Value = bgmVolume;
        PlayerPrefs.SetFloat(BgmVolumePrefKey, bgmVolume);
        PlayerPrefs.Save();
    }

    public void SetSEVolume(float volume)
    {
        seVolume = Mathf.Clamp01(volume);
        SeVolume.Value = seVolume;
        PlayerPrefs.SetFloat(SeVolumePrefKey, seVolume);
        PlayerPrefs.Save();
    }

    private IEnumerator FadeToNewBGM(SoundEntry entry, bool loop, float fadeTime)
    {
        float startVolume = bgmSource.volume;

        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        bgmSource.clip = entry.clip;
        bgmSource.pitch = entry.pitch;
        bgmSource.loop = loop;
        bgmSource.Play();

        float targetVolume = entry.volume * bgmVolume;
        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(0f, targetVolume, t / fadeTime);
            yield return null;
        }

        bgmSource.volume = targetVolume;
        fadeRoutine = null;
    }

    private IEnumerator FadeOutAndStop(float fadeTime)
    {
        float startVolume = bgmSource.volume;

        for (float t = 0f; t < fadeTime; t += Time.unscaledDeltaTime)
        {
            bgmSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeTime);
            yield return null;
        }

        bgmSource.Stop();
        bgmSource.volume = startVolume;
        fadeRoutine = null;
    }
}
