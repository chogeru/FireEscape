using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public enum OpenMode
{
    RotateHinge,
    SlideDrawer
}

/// <summary>
/// ドア(ヒンジ回転)・引き出し(スライド)の両方をカバーする開閉インタラクタブル。
/// 開いた状態は「初期姿勢からのオフセット」で指定するため、シーン上の初期回転・位置が
/// そのまま「閉」の基準になる(角度のラップアラウンドにも強いQuaternion補間)。
/// LoopManagerのフラグと連携して「このフラグが立つまで施錠」といった演出に使える。
/// </summary>
public class OpenableInteractable : InteractableBase
{
    [Header("Open Mode")]
    [SerializeField] private OpenMode mode = OpenMode.RotateHinge;
    [SerializeField] private float duration = 0.6f;

    [Header("Rotate (Door) - 閉状態からの相対回転")]
    [SerializeField] private Vector3 openEulerOffset = new(0f, 90f, 0f);

    [Header("Slide (Drawer) - 閉状態からの相対移動")]
    [SerializeField] private Vector3 openLocalOffset = new(0f, 0f, 0.4f);

    [Header("Lock")]
    [SerializeField] private bool locked;
    [Tooltip("これが空でなければ、LoopManagerでこのフラグが立っている時のみ解錠可能")]
    [SerializeField] private string requiredFlag;
    [SerializeField] private UnityEvent onInteractDenied;

    private bool isOpen;
    private Vector3 closedLocalPos;
    private Quaternion closedLocalRot;
    private Coroutine moveRoutine;

    private void Reset()
    {
        oneShot = false;
        gazeDuration = 0.4f;
    }

    private void Awake()
    {
        closedLocalPos = transform.localPosition;
        closedLocalRot = transform.localRotation;
    }

    public override void Interact(GameObject interactor)
    {
        if (locked)
        {
            if (!string.IsNullOrEmpty(requiredFlag) && LoopManager.Instance != null && LoopManager.Instance.HasFlag(requiredFlag))
            {
                locked = false;
            }
            else
            {
                onInteractDenied?.Invoke();
                return;
            }
        }

        Toggle();
        base.Interact(interactor);
    }

    public void SetLocked(bool value) => locked = value;

    private void Toggle()
    {
        isOpen = !isOpen;
        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        moveRoutine = StartCoroutine(MoveRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        float t = 0f;
        Quaternion startRot = transform.localRotation;
        Vector3 startPos = transform.localPosition;

        Quaternion targetRot = isOpen ? closedLocalRot * Quaternion.Euler(openEulerOffset) : closedLocalRot;
        Vector3 targetPos = isOpen ? closedLocalPos + openLocalOffset : closedLocalPos;

        while (t < duration)
        {
            t += Time.deltaTime;
            float f = Mathf.SmoothStep(0f, 1f, t / duration);

            if (mode == OpenMode.RotateHinge)
                transform.localRotation = Quaternion.Slerp(startRot, targetRot, f);
            else
                transform.localPosition = Vector3.Lerp(startPos, targetPos, f);

            yield return null;
        }

        if (mode == OpenMode.RotateHinge)
            transform.localRotation = targetRot;
        else
            transform.localPosition = targetPos;
    }
}
