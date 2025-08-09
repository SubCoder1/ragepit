using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 6f;
    public float runSpeed = 12f;
    public float jumpHeight = 2.3f;
    public float gravity = -20f;
    public float fallMultiplier = 3.8f;
    public float rotationSpeed = 10f;

    [Header("References")]
    public Transform cameraTransform;
    public Joystick moveJoystick;
    public InputActionReference jumpAction;

    [Header("Jump Timing")]
    public float coyoteTime = 0.08f;
    public float jumpInputBufferTime = 0.2f;  // Buffer time for jump input

    [Header("Sprint Lock UI")]
    [SerializeField] private GameObject sprintIcon;
    [SerializeField] private ThirdPersonCamera thirdPersonCamera;

    private CharacterController controller;
    private Animator animator;

    private Vector2 inputMovement;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isJumping;

    private float fallTimer = 0f;
    private float fallAnimationThreshold = 0.15f;
    private float lastGroundedTime;
    private Vector3 jumpMovementVelocity = Vector3.zero;
    private float landingLerpTimer = 0f;
    private float landingLerpDuration = 0.15f;
    private float lastJumpTime = -999f;

    private float jumpCooldown = 0.5f;
    private float landingTime = -999f;
    [SerializeField] private float landingBuffer = 0.25f;

    private Vector3 camForward;
    private Vector3 camRight;

    private bool isSprintLocked = false;
    [SerializeField] private float sprintTriggerY = 0.95f;
    [SerializeField] private float sprintCancelY = -0.3f;

    // Jump input buffer time tracking
    private float jumpInputTime = -999f;

    // New flag to control if player can jump
    private bool canJump = true;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        if (sprintIcon != null) sprintIcon.SetActive(false);
    }

    void OnEnable()
    {
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJumpPressed;
    }

    void OnDisable()
    {
        jumpAction.action.Disable();
        jumpAction.action.performed -= OnJumpPressed;
    }

    void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        // Only buffer jump input if player is allowed to jump
        if (!canJump) return;

        jumpInputTime = Time.time;
    }

    public void OnJumpButtonPressed()
    {
        if (!canJump) return;
        jumpInputTime = Time.time;
    }

    void LateUpdate()
    {
        if (cameraTransform == null || moveJoystick == null) return;

        float targetYawFromCamera = thirdPersonCamera.SmoothedYaw;
        Quaternion smoothTargetRotation = Quaternion.Euler(0f, targetYawFromCamera, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, smoothTargetRotation, 1 - Mathf.Exp(-15f * Time.deltaTime));

        UpdateCameraVectors();

        bool wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        if (isGrounded)
        {
            lastGroundedTime = Time.time;

            if (!wasGrounded)
            {
                landingTime = Time.time;
            }

            // Reset canJump when grounded
            canJump = true;
        }

        inputMovement = new Vector2(moveJoystick.Horizontal, moveJoystick.Vertical);
        float verticalInput = inputMovement.y;

        if (!isSprintLocked && verticalInput >= sprintTriggerY)
        {
            isSprintLocked = true;
            if (sprintIcon != null) sprintIcon.SetActive(true);
        }
        else if (isSprintLocked && verticalInput <= sprintCancelY)
        {
            isSprintLocked = false;
            if (sprintIcon != null) sprintIcon.SetActive(false);
        }

        Vector3 inputDir = new Vector3(inputMovement.x, 0f, inputMovement.y);
        bool hasInput = inputDir.sqrMagnitude > 0.01f;
        Vector3 moveDir = Vector3.zero;
        float moveSpeed = 0f;

        if (hasInput)
        {
            moveDir = camForward * inputMovement.y + camRight * inputMovement.x;
            moveSpeed = isSprintLocked ? runSpeed : walkSpeed;
        }
        else if (isSprintLocked)
        {
            moveDir = camForward;
            moveSpeed = runSpeed;
        }

        moveDir.Normalize();

        fallTimer = isGrounded ? 0f : fallTimer + Time.deltaTime;
        bool isActuallyFalling = !isGrounded && fallTimer > fallAnimationThreshold;

        Vector3 horizontalMove = Vector3.zero;

        if (isGrounded)
        {
            landingLerpTimer += Time.deltaTime;

            if (hasInput || isSprintLocked)
            {
                jumpMovementVelocity = moveDir * moveSpeed;
                horizontalMove = jumpMovementVelocity;
                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
            else
            {
                if (landingLerpTimer < landingLerpDuration)
                {
                    float t = landingLerpTimer / landingLerpDuration;
                    horizontalMove = Vector3.Lerp(jumpMovementVelocity, Vector3.zero, t);
                }
                else
                {
                    jumpMovementVelocity = Vector3.zero;
                }
            }

            float targetSpeed = moveSpeed == runSpeed ? 2f : moveSpeed == walkSpeed ? 1f : 0f;
            animator.SetFloat("Speed", targetSpeed, 0.15f, Time.deltaTime);

            if (!isJumping) velocity.y = -2f;

            // Process buffered jump input if conditions met
            if (jumpInputTime > 0f &&
                (Time.time - jumpInputTime) <= jumpInputBufferTime &&
                (Time.time - lastGroundedTime) <= coyoteTime &&
                canJump &&
                Time.time > lastJumpTime + jumpCooldown &&
                Time.time > landingTime + landingBuffer)
            {
                UpdateCameraVectors();

                Vector3 inputMoveDir = new Vector3(inputMovement.x, 0f, inputMovement.y);
                Vector3 jumpDir = camForward * inputMoveDir.z + camRight * inputMoveDir.x;
                float jumpSpeed = isSprintLocked ? runSpeed : walkSpeed;
                jumpMovementVelocity = jumpDir.normalized * jumpSpeed;

                StartCoroutine(HandleJump(jumpMovementVelocity));
                jumpInputTime = -999f;
                lastJumpTime = Time.time;
                canJump = false; // lock jumping until next landing
            }
        }
        else
        {
            Vector3 airInput = new Vector3(inputMovement.x, 0f, inputMovement.y);
            Vector3 airDir = camForward * airInput.z + camRight * airInput.x;

            if (airDir.sqrMagnitude > 0.01f)
            {
                Vector3 targetVel = airDir.normalized * walkSpeed;
                jumpMovementVelocity = Vector3.Lerp(jumpMovementVelocity, targetVel, Time.deltaTime * 1.5f);
                Quaternion airRot = Quaternion.LookRotation(airDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, airRot, Time.deltaTime * rotationSpeed);
            }

            jumpMovementVelocity = Vector3.ClampMagnitude(jumpMovementVelocity, runSpeed);

            if (isActuallyFalling) animator.SetFloat("Speed", 0f, 0.15f, Time.deltaTime);

            landingLerpTimer = 0f;
            velocity.y += (velocity.y < 0 ? gravity * fallMultiplier : gravity) * Time.deltaTime;
        }

        velocity.y = Mathf.Clamp(velocity.y, -100f, 100f);
        Vector3 finalMove = jumpMovementVelocity + Vector3.up * velocity.y;
        controller.Move(finalMove * Time.deltaTime);
    }

    private void UpdateCameraVectors()
    {
        camForward = cameraTransform.forward;
        camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();
    }

    IEnumerator HandleJump(Vector3 momentumAtJump)
    {
        isJumping = true;
        jumpMovementVelocity = momentumAtJump;

        float speedParam = animator.GetFloat("Speed");
        animator.SetInteger("jumpType", speedParam < 0.1f ? 0 : speedParam < 1.5f ? 1 : 2);
        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(0.18f);

        velocity.y = Mathf.Sqrt(jumpHeight * Mathf.Abs(gravity)) * 0.667f;

        yield return new WaitUntil(() => controller.isGrounded);
        isJumping = false;
    }
}
