using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// BGM/SE再生の統合ファサード。AudioManager.Instanceから呼び出す公開APIはこれまでと変わらないが、
/// 内部の実装は関心事ごとにpartial classとしてファイル分割している:
/// - AudioManager.Bgm.cs      BGM再生・クロスフェード・BGM音量
/// - AudioManager.SePool.cs   3D SEプールの再生・管理・SE音量
/// - AudioManager.Occlusion.cs 3D SEの障害物オクルージョン
/// 呼び出し側(FootstepController等)はこのクラスの分割を意識する必要はない。
/// </summary>
public partial class AudioManager : MonoSingleton<AudioManager>
{
    [FoldoutGroup("Sound Data"), SerializeField, Required] private SoundLibrary library;

    protected override void Awake()
    {
        base.Awake();
        InitializeBgmVolume();
        InitializeSeVolume();
        InitializeBgmSource();
        InitializeSePool();
    }

    private void Start()
    {
        StartInitialBgmIfConfigured();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        BgmVolume.Dispose();
        SeVolume.Dispose();
    }

    private void LateUpdate()
    {
        UpdateSePool();
    }
}
