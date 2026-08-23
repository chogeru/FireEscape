using Sirenix.OdinInspector;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// タイトル画面のDIコンポジションルート。IScreenFaderの実装と、シーン遷移の唯一の窓口である
/// GameSceneManagerを登録し、シーン上に既に配置されているTitleMenuControllerへ[Inject]経由で渡す。
/// フェード演出(IScreenFader)を差し替えたい場合はここだけを変更すればよい。
/// </summary>
public class TitleLifetimeScope : LifetimeScope
{
    [SerializeField, Required] private CanvasGroupFader fader;

    protected override void Configure(IContainerBuilder builder)
    {
        builder.RegisterComponent(fader).As<IScreenFader>();
        builder.RegisterComponent(GameSceneManager.Instance).As<ISceneTransitionService>();
        builder.RegisterComponentInHierarchy<TitleMenuController>();
    }
}
