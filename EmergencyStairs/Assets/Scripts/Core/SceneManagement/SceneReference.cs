using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// ロード先シーンをアセットとして表現するSO。文字列のシーン名をコード内に直接書かず、
/// Inspector上でシーンアセットをドラッグ&ドロップして割り当てる(タイポ防止・参照の一元管理)。
/// Build Settingsに未登録/無効の場合はInspector上に警告を出し、実行時エラーになる前に気付けるようにする。
/// </summary>
[CreateAssetMenu(menuName = "Scene Management/Scene Reference", fileName = "New Scene Reference")]
public class SceneReference : ScriptableObject
{
#if UNITY_EDITOR
    [SerializeField, Required, OnValueChanged(nameof(SyncSceneNameInEditor))]
    [InfoBox("$" + nameof(BuildSettingsWarning), InfoMessageType.Error, nameof(IsMissingFromBuildSettings))]
    private SceneAsset sceneAsset;
#endif

    [SerializeField, ReadOnly] private string sceneName;

    public string SceneName => sceneName;

#if UNITY_EDITOR
    private void OnValidate() => SyncSceneNameInEditor();

    private void SyncSceneNameInEditor()
    {
        sceneName = sceneAsset != null ? sceneAsset.name : null;
    }

    private bool IsMissingFromBuildSettings()
    {
        if (sceneAsset == null) return false;

        string path = AssetDatabase.GetAssetPath(sceneAsset);
        foreach (var entry in EditorBuildSettings.scenes)
        {
            if (entry.enabled && entry.path == path)
                return false;
        }
        return true;
    }

    private string BuildSettingsWarning =>
        "このシーンはBuild Settingsに含まれていないか無効化されています。実行時にロードできません。";
#endif
}
