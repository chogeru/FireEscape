using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

/// <summary>
/// 2枚のIPanelを排他的に切り替える汎用コンポーネント。
/// 「メインメニュー⇔オプション」のように常にどちらか一方だけを表示する画面遷移パターンを
/// 使い回すためのもの。ボタンのonClickから直接ShowPrimary/ShowSecondaryを呼べばよく、
/// 呼び出し元の画面コントローラーはパネルの存在を一切知らなくてよい。
/// EventSystemが使うUI Cancelアクション(Escキー/ゲームパッドBボタン等)も自動で購読し、
/// secondaryPanel表示中に押されたらprimaryPanelへ戻す。
/// </summary>
public class TwoPanelNavigator : MonoBehaviour
{
    [SerializeField, Required] private MenuPanel primaryPanel;
    [SerializeField, Required] private MenuPanel secondaryPanel;

    private InputSystemUIInputModule uiModule;

    private void Start()
    {
        uiModule = EventSystem.current != null ? EventSystem.current.currentInputModule as InputSystemUIInputModule : null;
        if (uiModule != null && uiModule.cancel != null && uiModule.cancel.action != null)
            uiModule.cancel.action.performed += OnCancelPerformed;

        ShowPrimary();
    }

    private void OnDestroy()
    {
        if (uiModule != null && uiModule.cancel != null && uiModule.cancel.action != null)
            uiModule.cancel.action.performed -= OnCancelPerformed;
    }

    private void OnCancelPerformed(InputAction.CallbackContext ctx)
    {
        if (secondaryPanel != null && secondaryPanel.IsOpen.Value)
            ShowPrimary();
    }

    public void ShowPrimary()
    {
        secondaryPanel?.Close();
        primaryPanel?.Open();
    }

    public void ShowSecondary()
    {
        primaryPanel?.Close();
        secondaryPanel?.Open();
    }
}
