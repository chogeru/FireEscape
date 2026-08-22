using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 「見つめる」だけで発火するインタラクト対象の基底クラス(写真、貼り紙、拾えるアイテムなど)。
/// ドアや引き出しのように繰り返し使えるものはoneShotをfalseにしてInteract()をoverrideする。
/// </summary>
public class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] protected bool canInteract = true;
    [Tooltip("見つめ続ける必要がある秒数。0なら視線が合った瞬間に発火")]
    [SerializeField] protected float gazeDuration = 1.0f;
    [Tooltip("ONの場合、一度発火したら二度と反応しなくなる(調べ物・発見系向け)")]
    [SerializeField] protected bool oneShot = true;
    [SerializeField] protected UnityEvent onInteract;

    public virtual bool CanInteract => canInteract;
    public float GazeDuration => gazeDuration;

    public virtual void Interact(GameObject interactor)
    {
        if (oneShot)
            canInteract = false;

        onInteract?.Invoke();
    }
}
