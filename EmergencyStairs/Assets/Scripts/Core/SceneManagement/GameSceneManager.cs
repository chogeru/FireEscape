using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TransitionsPlus;
using UnityEngine;

/// <summary>
/// アプリ全体のシーン遷移を一元管理するクラス。Transitions Plusで演出しつつ読み込む、
/// ISceneTransitionServiceの標準実装。タイトル/ポーズ/クリア画面など、シーン遷移が必要な
/// どの場所からもここを経由させることで、各画面のコントローラーが演出やSceneManager.LoadSceneを
/// 直接扱わずに済む(遷移先はSceneReferenceで渡すためコード中に生の文字列も出てこない)。
/// AudioManager等と同じくDontDestroyOnLoadで常駐し、シーンを跨いで唯一のインスタンスであり続ける。
/// </summary>
public class GameSceneManager : MonoSingleton<GameSceneManager>, ISceneTransitionService
{
    [SerializeField, Required, Tooltip("遷移演出を個別指定しなかった場合に使うデフォルトのプロファイル")]
    private TransitionProfile defaultTransition;

    public UniTask LoadSceneAsync(SceneReference scene, CancellationToken ct = default)
        => LoadSceneAsync(scene, defaultTransition, ct);

    public async UniTask LoadSceneAsync(SceneReference scene, TransitionProfile transition, CancellationToken ct = default)
    {
        if (scene == null || string.IsNullOrEmpty(scene.SceneName))
        {
            Debug.LogError("[GameSceneManager] SceneReference is not set.");
            return;
        }

        TransitionProfile profile = transition != null ? transition : defaultTransition;
        if (profile == null)
        {
            Debug.LogError("[GameSceneManager] No TransitionProfile available.");
            return;
        }

        var tcs = new UniTaskCompletionSource();
        TransitionAnimator animator = TransitionAnimator.Start(profile, autoDestroy: true, sceneNameToLoad: scene.SceneName);

        void OnTransitionEnd() => tcs.TrySetResult();
        animator.onTransitionEnd.AddListener(OnTransitionEnd);

        using (ct.Register(() => tcs.TrySetCanceled()))
        {
            try
            {
                await tcs.Task;
            }
            finally
            {
                if (animator != null)
                    animator.onTransitionEnd.RemoveListener(OnTransitionEnd);
            }
        }
    }
}
