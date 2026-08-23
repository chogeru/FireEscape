using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;

/// <summary>
/// タイトル画面の「開始」フロー専任コントローラー。フェード演出とシーン遷移は
/// VContainer経由で注入されるモジュール(IScreenFader, ISceneTransitionService)に委譲する。
/// シーン遷移そのものはGameSceneManagerの責務であり、ここでは「どのSceneReferenceへ
/// 行きたいか」を伝えるだけで、SceneManager.LoadScene等を直接呼ぶことは一切しない。
/// パネル切り替えはTwoPanelNavigator、音量スライダーはAudioVolumeSliderBinding、
/// 終了処理はApplicationQuitterがそれぞれ単独で担当するため、ここでは開始フローのみに責任を絞る。
/// </summary>
public class TitleMenuController : MonoBehaviour
{
    [SerializeField, Required] private SceneReference gameScene;

    private IScreenFader fader;
    private ISceneTransitionService sceneTransition;

    private CancellationTokenSource lifetimeCts;

    [Inject]
    public void Construct(IScreenFader fader, ISceneTransitionService sceneTransition)
    {
        this.fader = fader;
        this.sceneTransition = sceneTransition;
    }

    private void Awake()
    {
        lifetimeCts = new CancellationTokenSource();
    }

    private void Start()
    {
        FadeInAsync(lifetimeCts.Token).Forget();
    }

    private async UniTaskVoid FadeInAsync(CancellationToken ct)
    {
        if (fader == null) return;
        await fader.FadeAsync(1f, 0f, ct);
    }

    public void OnStartPressed() => LoadGameAsync().Forget();

    private async UniTaskVoid LoadGameAsync()
    {
        if (sceneTransition == null)
        {
            Debug.LogError("[TitleMenuController] ISceneTransitionService is not injected.");
            return;
        }

        await sceneTransition.LoadSceneAsync(gameScene, lifetimeCts.Token);
    }

    private void OnDestroy()
    {
        lifetimeCts?.Cancel();
        lifetimeCts?.Dispose();
    }
}
