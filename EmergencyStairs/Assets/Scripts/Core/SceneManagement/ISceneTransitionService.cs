using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 演出を挟んだシーン遷移を行うモジュールの共通インターフェース。
/// 遷移先は生の文字列ではなくSceneReference(SO)で指定する。
/// </summary>
public interface ISceneTransitionService
{
    UniTask LoadSceneAsync(SceneReference scene, CancellationToken ct = default);
}
