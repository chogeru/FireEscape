using System.Threading;
using Cysharp.Threading.Tasks;

/// <summary>
/// 画面全体のフェード演出を行うモジュールの共通インターフェース。
/// タイトル/ポーズ/ゲームオーバーなど、シーン遷移を伴う画面はすべてこれ経由で暗転させる。
/// </summary>
public interface IScreenFader
{
    /// <summary>alpha(0-1)をfromからtoへ、実装側が持つ時間で補間する。</summary>
    UniTask FadeAsync(float from, float to, CancellationToken ct = default);
}
