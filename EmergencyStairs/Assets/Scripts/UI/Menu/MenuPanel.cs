using R3;
using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// GameObjectのアクティブ切り替えをIPanelとして公開する汎用パネル実装。
/// メインメニュー/オプション/ポーズ画面などどれも同じコンポーネントを使い回せる。
/// defaultSelectionを設定しておくと、Open()時に自動でその要素へゲームパッド/キーボードの
/// フォーカスを移す(コントローラー操作時にマウスで一度触るまで何も選択されない問題を防ぐ)。
/// </summary>
public class MenuPanel : MonoBehaviour, IPanel
{
    [SerializeField, Required] private GameObject root;
    [Tooltip("Open()時にコントローラー/キーボードの初期フォーカスを合わせる対象(未設定なら何もしない)")]
    [SerializeField] private GameObject defaultSelection;

    [ShowInInspector, ReadOnly]
    public ReactiveProperty<bool> IsOpen { get; } = new(false);

    private void Reset()
    {
        root = gameObject;
    }

    private void Awake()
    {
        if (root == null) root = gameObject;
        IsOpen.Value = root.activeSelf;
    }

    private void OnDestroy()
    {
        IsOpen.Dispose();
    }

    public void Open()
    {
        root.SetActive(true);
        IsOpen.Value = true;

        if (defaultSelection != null && UnityEngine.EventSystems.EventSystem.current != null)
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(defaultSelection);
    }

    public void Close()
    {
        root.SetActive(false);
        IsOpen.Value = false;
    }
}
