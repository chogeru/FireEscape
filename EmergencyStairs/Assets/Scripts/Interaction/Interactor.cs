using System;
using R3;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// プレイヤーのカメラに付ける注視インタラクションシステム。
/// キー入力は使わず、対象を一定時間見つめると自動的にIInteractable.Interact()が発火する。
/// (場所ベースのイベントはTriggerEventZoneを使う)
/// GameInputManager.GameplayInputEnabledを購読し、ゲームプレイ入力が停止している間
/// (ポーズ中・電話イベント中等)は毎フレームのポーリングなしで判定自体を止める。
/// </summary>
public class Interactor : MonoBehaviour
{
    [SerializeField] private Camera viewCamera;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactMask = ~0;
    [Tooltip("このTransform配下のコライダーはヒット対象から除外する(未設定なら自分のtransform.rootを自動使用し、自分自身への誤ヒットを防ぐ)")]
    [SerializeField] private Transform ignoreRoot;

    [Header("Events (UIに接続する)")]
    [SerializeField] private UnityEvent<float> onGazeProgress;
    [SerializeField] private UnityEvent onFocusLost;

    private IInteractable currentInteractable;
    private Highlightable currentHighlight;
    private float gazeTimer;
    private bool hasTriggeredCurrent;

    private bool gameplayInputEnabled = true;
    private IDisposable gameplayInputSubscription;

    private void Awake()
    {
        if (viewCamera == null)
            viewCamera = GetComponent<Camera>() != null ? GetComponent<Camera>() : Camera.main;
        if (ignoreRoot == null)
            ignoreRoot = transform.root;
    }

    private void OnEnable()
    {
        if (GameInputManager.Instance != null)
            gameplayInputSubscription = GameInputManager.Instance.GameplayInputEnabled.Subscribe(enabled => OnGameplayInputEnabledChanged(enabled));
    }

    private void OnDisable()
    {
        gameplayInputSubscription?.Dispose();
        gameplayInputSubscription = null;
    }

    private void OnGameplayInputEnabledChanged(bool enabled)
    {
        gameplayInputEnabled = enabled;
        if (!enabled) ClearFocus();
    }

    private void Update()
    {
        if (!gameplayInputEnabled) return;

        UpdateFocus();
        UpdateGazeTrigger();
    }

    private void UpdateFocus()
    {
        IInteractable hitInteractable = null;
        Highlightable hitHighlight = null;

        if (viewCamera != null &&
            Physics.Raycast(viewCamera.transform.position, viewCamera.transform.forward, out var hit, interactRange, interactMask) &&
            hit.collider.transform.root != ignoreRoot)
        {
            hitInteractable = hit.collider.GetComponentInParent<IInteractable>();
            hitHighlight = hit.collider.GetComponentInParent<Highlightable>();
        }

        if (hitInteractable == currentInteractable)
            return;

        if (currentHighlight != null)
            currentHighlight.SetHighlighted(false);

        currentInteractable = hitInteractable;
        currentHighlight = hitHighlight;
        gazeTimer = 0f;
        hasTriggeredCurrent = false;

        if (currentInteractable != null)
        {
            currentHighlight?.SetHighlighted(true);
        }
        else
        {
            onFocusLost?.Invoke();
            onGazeProgress?.Invoke(0f);
        }
    }

    private void UpdateGazeTrigger()
    {
        if (currentInteractable == null || !currentInteractable.CanInteract || hasTriggeredCurrent)
            return;

        gazeTimer += Time.deltaTime;
        float duration = currentInteractable.GazeDuration;
        float progress = duration > 0f ? Mathf.Clamp01(gazeTimer / duration) : 1f;
        onGazeProgress?.Invoke(progress);

        if (gazeTimer >= duration)
        {
            hasTriggeredCurrent = true;
            currentInteractable.Interact(gameObject);
        }
    }

    private void ClearFocus()
    {
        if (currentInteractable == null) return;

        currentHighlight?.SetHighlighted(false);
        currentInteractable = null;
        currentHighlight = null;
        gazeTimer = 0f;
        hasTriggeredCurrent = false;
        onFocusLost?.Invoke();
        onGazeProgress?.Invoke(0f);
    }
}
