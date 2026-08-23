using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SoundEntry
{
    public string id;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    [Range(0.1f, 3f)] public float pitch = 1f;

    [Header("3D Sound (SE only)")]
    [Tooltip("ONにすると再生位置に応じて音量・定位が変化する立体音響になります。BGMやUI音はOFF推奨。")]
    public bool is3D = true;
    [Range(0f, 1f)] public float spatialBlend = 1f;
    public float minDistance = 1f;
    public float maxDistance = 20f;
    public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;
}

[CreateAssetMenu(fileName = "SoundLibrary", menuName = "Audio/Sound Library")]
public class SoundLibrary : ScriptableObject
{
    [SerializeField] private List<SoundEntry> bgmEntries = new();
    [SerializeField] private List<SoundEntry> seEntries = new();

    /// <summary>登録済みBGM IDの一覧(Odinのドロップダウン等、Editorツール向け)。</summary>
    public IEnumerable<string> BgmIds => bgmEntries.Select(e => e.id);

    /// <summary>登録済みSE IDの一覧(Odinのドロップダウン等、Editorツール向け)。</summary>
    public IEnumerable<string> SeIds => seEntries.Select(e => e.id);

    private Dictionary<string, SoundEntry> bgmLookup;
    private Dictionary<string, SoundEntry> seLookup;

    private void OnEnable()
    {
        bgmLookup = Build(bgmEntries);
        seLookup = Build(seEntries);
    }

    private static Dictionary<string, SoundEntry> Build(List<SoundEntry> entries)
    {
        var dict = new Dictionary<string, SoundEntry>();
        foreach (var entry in entries)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id) || entry.clip == null)
                continue;
            dict[entry.id] = entry;
        }
        return dict;
    }

    public bool TryGetBGM(string id, out SoundEntry entry)
    {
        bgmLookup ??= Build(bgmEntries);
        return bgmLookup.TryGetValue(id, out entry);
    }

    public bool TryGetSE(string id, out SoundEntry entry)
    {
        seLookup ??= Build(seEntries);
        return seLookup.TryGetValue(id, out entry);
    }
}
