using R3;

/// <summary>
/// 開閉トグル可能なUIパネルの共通インターフェース。メインメニュー/オプション/ポーズなど
/// 同じ開閉ロジックを使い回すための抽象。IsOpenは外部からは読み取り専用として扱うこと。
/// </summary>
public interface IPanel
{
    ReactiveProperty<bool> IsOpen { get; }
    void Open();
    void Close();
}
