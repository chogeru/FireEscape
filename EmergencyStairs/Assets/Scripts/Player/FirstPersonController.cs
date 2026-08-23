using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Move")]
    [SerializeField] private float walkSpeed = 2.6f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float jumpHeight = 1.2f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float moveSmoothTime = 0.25f;

    [Header("Look")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private float mouseSensitivity = 2f;
    [SerializeField] private float minPitch = -85f;
    [SerializeField] private float maxPitch = 85f;
    [SerializeField] private float lookSmoothTime = 0.08f;

    private CharacterController controller;
    private Vector3 velocity;
    private float pitch;

    private Vector2 currentMoveInput;
    private Vector2 moveInputVelocity;

    private Vector2 smoothedLookDelta;
    private Vector2 lookDeltaVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (playerCamera == null)
            playerCamera = GetComponentInChildren<Camera>();
    }

    private void Update()
    {
        // カーソルのロック/解除はGameInputManagerが一元管理する(ポーズ/電話イベント等と競合しないように)。
        bool gameplayEnabled = GameInputManager.Instance == null || GameInputManager.Instance.IsGameplayInputEnabled;

        if (gameplayEnabled)
            HandleLook();

        HandleMove(gameplayEnabled);
    }

    private void HandleLook()
    {
        var mouse = Mouse.current;
        if (mouse == null) return;

        Vector2 rawDelta = mouse.delta.ReadValue() * mouseSensitivity * 0.1f;
        smoothedLookDelta = Vector2.SmoothDamp(smoothedLookDelta, rawDelta, ref lookDeltaVelocity, lookSmoothTime);
        Vector2 delta = smoothedLookDelta;

        transform.Rotate(Vector3.up, delta.x);

        pitch = Mathf.Clamp(pitch - delta.y, minPitch, maxPitch);
        if (playerCamera != null)
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleMove(bool gameplayEnabled)
    {
        var keyboard = gameplayEnabled ? Keyboard.current : null;
        Vector2 input = Vector2.zero;
        bool running = false;

        if (keyboard != null)
        {
            if (keyboard.wKey.isPressed) input.y += 1f;
            if (keyboard.sKey.isPressed) input.y -= 1f;
            if (keyboard.dKey.isPressed) input.x += 1f;
            if (keyboard.aKey.isPressed) input.x -= 1f;
            running = keyboard.leftShiftKey.isPressed;
        }
        input = Vector2.ClampMagnitude(input, 1f);

        currentMoveInput = Vector2.SmoothDamp(currentMoveInput, input, ref moveInputVelocity, moveSmoothTime);

        float speed = running ? runSpeed : walkSpeed;
        Vector3 move = (transform.right * currentMoveInput.x + transform.forward * currentMoveInput.y) * speed;

        if (controller.isGrounded)
        {
            if (velocity.y < 0f)
                velocity.y = -2f;

            if (keyboard != null && keyboard.spaceKey.wasPressedThisFrame)
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // 入力が止められていてもゲームプレイ入力停止中に足元が崩れる床にいれば落下は続けたいので、重力は常時適用する。
        velocity.y += gravity * Time.deltaTime;

        Vector3 motion = move + Vector3.up * velocity.y;
        controller.Move(motion * Time.deltaTime);
    }
}
