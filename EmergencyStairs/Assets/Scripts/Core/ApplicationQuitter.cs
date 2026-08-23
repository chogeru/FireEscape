using UnityEngine;

/// <summary>
/// アプリケーション終了処理。Editor実行中は再生停止、ビルドではApplication.Quit()という
/// 分岐をここに閉じ込め、どのメニューのQuitボタンからも同じメソッドを呼べば済むようにする。
/// </summary>
public class ApplicationQuitter : MonoBehaviour
{
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
