using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// トリガーコライダーに汎用イベントを紐付ける。
/// BGM切替、ドア施錠、照明変化、ジャンプスケアなど、Inspector上でUnityEventに
/// 既存コンポーネントのメソッド(AudioManager.PlayBGM、OpenableInteractable.SetLockedなど)を
/// 直接ドラッグ&ドロップして繋ぐだけで演出を量産できる。
/// </summary>
[RequireComponent(typeof(Collider))]
public class TriggerEventZone : MonoBehaviour
{
    [SerializeField] private string requiredTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    [SerializeField] private UnityEvent onTriggerEnter;
    [SerializeField] private UnityEvent onTriggerExit;

    private bool hasTriggered;

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggerOnce && hasTriggered) return;
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;

        hasTriggered = true;
        onTriggerEnter?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!string.IsNullOrEmpty(requiredTag) && !other.CompareTag(requiredTag)) return;
        onTriggerExit?.Invoke();
    }
}
