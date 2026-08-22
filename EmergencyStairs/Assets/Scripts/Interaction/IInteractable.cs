using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }

    /// <summary>この時間だけ見つめ続けると自動的にInteract()が呼ばれる(0なら見た瞬間に即発火)。</summary>
    float GazeDuration { get; }

    void Interact(GameObject interactor);
}
