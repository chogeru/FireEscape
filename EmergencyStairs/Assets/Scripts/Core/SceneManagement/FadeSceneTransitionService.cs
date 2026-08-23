using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// IScreenFaderで暗転してからシーンを読み込む、ISceneTransitionServiceの軽量な標準実装
/// (演出アセット不要のシンプルなCanvasGroupフェードのみ)。Transitions Plusを使わない
/// 画面ではこちらを差し込める。
/// </summary>
public class FadeSceneTransitionService : MonoBehaviour, ISceneTransitionService
{
    [SerializeField, Required] private CanvasGroupFader fader;

    private IScreenFader Fader => fader;

    public async UniTask LoadSceneAsync(SceneReference scene, CancellationToken ct = default)
    {
        if (scene == null || string.IsNullOrEmpty(scene.SceneName))
        {
            Debug.LogError("[FadeSceneTransitionService] SceneReference is not set.");
            return;
        }

        if (Fader != null)
            await Fader.FadeAsync(0f, 1f, ct);

        await SceneManager.LoadSceneAsync(scene.SceneName).ToUniTask(cancellationToken: ct);
    }
}
