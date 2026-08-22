using UnityEngine;

/// <summary>
/// 足元の床材質を判定して対応する足音SEを再生する。
/// SoundLibraryのSEには "Footstep_Wood" のように "Footstep_" + FloorMaterial名 でIDを登録しておく。
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class FootstepController : MonoBehaviour
{
    [SerializeField] private Transform feet;
    [SerializeField] private float walkStepInterval = 0.5f;
    [SerializeField] private float runStepInterval = 0.32f;
    [SerializeField] private float runSpeedThreshold = 5f;
    [SerializeField] private float rayDistance = 0.6f;
    [SerializeField] private LayerMask groundMask = ~0;
    [SerializeField] private FloorMaterial defaultMaterial = FloorMaterial.Concrete;
    [SerializeField] private string idPrefix = "Footstep_";

    private CharacterController controller;
    private float stepTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (feet == null)
            feet = transform;
    }

    private void Update()
    {
        if (!controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        Vector3 horizontalVelocity = new(controller.velocity.x, 0f, controller.velocity.z);
        float speed = horizontalVelocity.magnitude;

        if (speed < 0.1f)
        {
            stepTimer = 0f;
            return;
        }

        stepTimer -= Time.deltaTime;
        if (stepTimer <= 0f)
        {
            PlayFootstep();
            stepTimer = speed >= runSpeedThreshold ? runStepInterval : walkStepInterval;
        }
    }

    private void PlayFootstep()
    {
        if (AudioManager.Instance == null) return;

        FloorMaterial material = defaultMaterial;
        if (Physics.Raycast(feet.position + Vector3.up * 0.1f, Vector3.down, out var hit, rayDistance, groundMask))
        {
            var tag = hit.collider.GetComponent<FloorMaterialTag>();
            if (tag != null)
                material = tag.material;
        }

        AudioManager.Instance.PlaySEAtPoint(idPrefix + material, feet.position);
    }
}
