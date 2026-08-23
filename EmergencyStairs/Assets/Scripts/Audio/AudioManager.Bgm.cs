using System.Collections;
using R3;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>AudioManagerのうち、BGM再生・クロスフェード・BGM音量を担当する部分。</summary>
public partial class AudioManager
{
    [FoldoutGroup("Sound Data"), Tooltip("設定するとStart()で自動的にBGM再生を開始する(未設定なら何もしない)。")]
    [SerializeField, ValueDropdown(nameof(GetBgmIdOptions))] private string startBgmId;
    [FoldoutGroup("Sound Data"), SerializeField] private float startBgmFadeTime = 2f;

#if UNITY_EDITOR
    private static System.Collections.Generic.IEnumerable<string> GetBgmIdOptions() => SoundLibraryIds.BgmIds();
#else
    private static System.Collections.Generic.IEnumerable<string> GetBgmIdOptions() => System.Linq.Enumerable.Empty<string>();
#endif

    [FoldoutGroup("Sources"), SerializeField] private AudioSource bgmSource;

    [FoldoutGroup("Volume"), SerializeField, Range(0f, 1f)] private float bgmVolume = 1f;

    /// <summary>現在のBGM音量(0-1)。値の変更を購読すれば、オプション画面のスライダー等をリアクティブに同期できる。</summary>
    public ReactiveProperty<float> BgmVolume { get; } = new(1f);

    [FoldoutGroup("Volume"), ShowInInspector, ReadOnly, ProgressBar(0, 1), LabelText("Bgm Volume (live)")]
    private float BgmVolumeDisplay => BgmVolume.Value;

    private const string BgmVolumePrefKey = "AudioManager.BgmVolume";

    private string currentBgmId;
    private Coroutine fadeRoutine;

    private void InitializeBgmVolume()
    {
        if (PlayerPrefs.HasKey(BgmVolumePrefKey)) bgmVolume = PlayerPrefs.GetFloat(BgmVolumePrefKey);
        BgmVolume.Value = bgmVolume;
    }

    private void InitializeBgmSource()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.spatialBlend = 0f;
    }

    private void StartInitialBgmIfConfigured()
    {
        if (!string.IsNullOrEmpty(startBgmId))
            PlayBGM(startBgmId, true, startBgmFadeTime);
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

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        bgmSource.volume = bgmVolume;
        BgmVolume.Value = bgmVolume;
        PlayerPrefs.SetFloat(BgmVolumePrefKey, bgmVolume);
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
