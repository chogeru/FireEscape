using UnityEngine;

/// <summary>
/// 指定タグを持つ対象を指定地点へ瞬間移動させる汎用コンポーネント。
/// ループ地点での巻き戻しに限らず、テレポートパッド等どんな用途にも使い回せる。
/// 単体では何もしない(公開メソッドを呼ぶだけ)ので、TriggerEventZoneのUnityEventや
/// 他のイベントから TeleportTaggedTarget() を呼んで使う。
/// </summary>
public class TeleportOnTrigger : MonoBehaviour
{
    [SerializeField] private Transform destination;
    [SerializeField] private string targetTag = "Player";

    public void TeleportTaggedTarget()
    {
        var target = GameObject.FindGameObjectWithTag(targetTag);
        if (target == null) return;
        Teleport(target.transform);
    }

    public void Teleport(Transform target)
    {
        if (target == null || destination == null) return;

        var controller = target.GetComponent<CharacterController>();
        if (controller != null) controller.enabled = false;

        target.SetPositionAndRotation(destination.position, destination.rotation);

        if (controller != null) controller.enabled = true;
    }
}
