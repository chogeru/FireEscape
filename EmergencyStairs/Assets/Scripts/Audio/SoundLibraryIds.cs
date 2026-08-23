#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

/// <summary>
/// Odinの[ValueDropdown]用に、プロジェクト内のSoundLibraryに登録済みのBGM/SE IDを列挙するヘルパー。
/// 音IDをInspectorで手入力させず候補から選ばせることで、タイポによる再生失敗を防ぐ。
/// Editor専用(#if UNITY_EDITOR)なのでビルドには含まれない。
/// </summary>
public static class SoundLibraryIds
{
    private static SoundLibrary cached;

    private static SoundLibrary Library
    {
        get
        {
            if (cached != null) return cached;
            var guids = AssetDatabase.FindAssets("t:SoundLibrary");
            if (guids.Length == 0) return null;
            cached = AssetDatabase.LoadAssetAtPath<SoundLibrary>(AssetDatabase.GUIDToAssetPath(guids[0]));
            return cached;
        }
    }

    public static IEnumerable<string> BgmIds() => Library != null ? Library.BgmIds : Enumerable.Empty<string>();
    public static IEnumerable<string> SeIds() => Library != null ? Library.SeIds : Enumerable.Empty<string>();
}
#endif
