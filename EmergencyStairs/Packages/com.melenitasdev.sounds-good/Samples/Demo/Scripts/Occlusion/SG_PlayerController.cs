using UnityEngine;

namespace MelenitasDev.SoundsGood.Demo
{
    [RequireComponent(typeof(CharacterController))]
    public class SG_PlayerController : MonoBehaviour
    {
        // ----- Serialized Fields
        [Header("References")]
        [SerializeField] private Transform cameraRoot;
        [SerializeField] private Transform cameraTransform;

        [Header("Movement")]
        [SerializeField] private float walkSpeed = 4f;
        [SerializeField] private float runSpeed = 7f;
        [SerializeField] private float acceleration = 10f;
        [SerializeField] private float gravity = -18f;
        [SerializeField] private float jumpHeight = 1.5f;

        [Header("Mouse Look")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float verticalClamp = 85f;
        [SerializeField] private float mouseSmooth = 12f;

        [Header("Head Bobbing")]
        [SerializeField] private float bobFrequency = 1.8f;
        [SerializeField] private float bobAmplitude = 0.05f;
        [SerializeField] private float bobSmoothing = 10f;

        [Header("Tilt & Sway")]
        [SerializeField] private float tiltAngle = 5f;
        [SerializeField] private float swayAmount = 1.5f;
        [SerializeField] private float swaySmoothing = 10f;


        // ----- Fields
        private CharacterController controller;

        private float yaw;
        private float pitch;
        private float smoothedYaw;
        private float smoothedPitch;

        private Vector3 velocity;
        private Vector3 moveVelocity;

        private float bobTimer;
        private Vector3 cameraRootDefaultLocalPos;

        private Vector3 targetLocalEuler;
        private Vector3 currentLocalEuler;

        // ----- Unity Events
        void Awake ()
        {
            controller = GetComponent<CharacterController>();

            if (cameraRoot == null && Camera.main != null)
            {
                cameraRoot = Camera.main.transform;
            }

            if (cameraTransform == null && cameraRoot != null)
            {
                cameraTransform = cameraRoot;
            }

            if (cameraRoot != null)
            {
                cameraRootDefaultLocalPos = cameraRoot.localPosition;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            float startYaw = transform.eulerAngles.y;
            yaw = startYaw;
            smoothedYaw = startYaw;
            pitch = 0f;
            smoothedPitch = 0f;
            currentLocalEuler = Vector3.zero;
        }

        void Update ()
        {
            HandleMouseLook();
            HandleMovement();
            HandleHeadBob();
            HandleTiltAndSway();
        }

        // ----- Private Methods
        private void HandleMouseLook ()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
            
            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, -verticalClamp, verticalClamp);
            
            smoothedYaw = Mathf.LerpAngle(smoothedYaw, yaw, mouseSmooth * Time.deltaTime);
            smoothedPitch = Mathf.Lerp(smoothedPitch, pitch, mouseSmooth * Time.deltaTime);
            
            transform.rotation = Quaternion.Euler(0f, smoothedYaw, 0f);
        }

        private void HandleMovement ()
        {
            bool isGrounded = controller.isGrounded;

            float inputX = Input.GetAxisRaw("Horizontal");
            float inputZ = Input.GetAxisRaw("Vertical");

            Vector3 inputDir = new Vector3(inputX, 0f, inputZ);
            inputDir = Vector3.ClampMagnitude(inputDir, 1f);

            float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
            Vector3 targetVelocity = transform.TransformDirection(inputDir) * targetSpeed;
            
            moveVelocity = Vector3.Lerp(moveVelocity, targetVelocity, acceleration * Time.deltaTime);
            
            if (isGrounded && velocity.y < 0f)
            {
                velocity.y = -2f;
            }

            if (isGrounded && Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }

            velocity.y += gravity * Time.deltaTime;

            Vector3 finalMove = moveVelocity;
            finalMove.y = velocity.y;

            controller.Move(finalMove * Time.deltaTime);
        }

        private void HandleHeadBob ()
        {
            if (cameraRoot == null) return;

            Vector3 horizontalVel = moveVelocity;
            horizontalVel.y = 0f;

            float speed = horizontalVel.magnitude;
            bool isMoving = speed > 0.1f && controller.isGrounded;
            
            if (isMoving)
            {
                float speedFactor = speed / walkSpeed;
                bobTimer += Time.deltaTime * bobFrequency * Mathf.Max(speedFactor, 0.1f);
            }

            float bobOffset = isMoving
                ? Mathf.Sin(bobTimer * Mathf.PI * 2f) * bobAmplitude
                : 0f;

            Vector3 targetPos = cameraRootDefaultLocalPos + new Vector3(0f, bobOffset, 0f);

            cameraRoot.localPosition = Vector3.Lerp(
                cameraRoot.localPosition,
                targetPos,
                Time.deltaTime * bobSmoothing
            );
        }

        private void HandleTiltAndSway ()
        {
            if (cameraRoot == null) return;
            
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");
            
            float targetRoll = -mouseX * tiltAngle;
            
            float targetSwayYaw = mouseX * swayAmount;
            float targetSwayPitch = -mouseY * swayAmount * 0.5f;
            
            targetLocalEuler.x = smoothedPitch + targetSwayPitch;
            targetLocalEuler.y = targetSwayYaw;
            targetLocalEuler.z = targetRoll;

            currentLocalEuler = Vector3.Lerp(currentLocalEuler, targetLocalEuler, 
                Time.deltaTime * swaySmoothing);

            cameraRoot.localRotation = Quaternion.Euler(currentLocalEuler);
        }
    }
}