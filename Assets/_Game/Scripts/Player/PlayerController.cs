using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 12f;
    public float jumpHeight = 3.3f;
    public float gravity = -20f;
    public float rotationSpeed = 10f;
    public float jumpDelay = 0.15f;

    [Header("References")]
    public Transform cameraTransform;
    public InputActionReference moveAction;
    public InputActionReference runAction;
    public InputActionReference jumpAction;

    private CharacterController controller;
    private Animator animator;

    private Vector2 inputMovement;
    private bool isRunning;
    private Vector3 velocity;
    private bool isGrounded;
    private bool jumpRequested;
    private bool isJumping;
    private bool hasMovementDelayPassed = false;

    private float fallTimer = 0f;
    private float fallAnimationThreshold = 0.15f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
    }

    void OnEnable()
    {
        moveAction.action.Enable();
        runAction.action.Enable();
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJumpPressed;
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        runAction.action.Disable();
        jumpAction.action.Disable();
        jumpAction.action.performed -= OnJumpPressed;
    }

    void OnJumpPressed(InputAction.CallbackContext ctx)
    {
        if (isGrounded && !isJumping)
        {
            jumpRequested = true;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        bool wasGrounded = isGrounded;
        isGrounded = controller.isGrounded;

        inputMovement = moveAction.action.ReadValue<Vector2>();
        isRunning = runAction.action.IsPressed();
        Vector3 inputDir = new Vector3(inputMovement.x, 0f, inputMovement.y);
        bool hasInput = inputDir.sqrMagnitude > 0.01f;

        Vector3 camForward = cameraTransform.forward;
        Vector3 camRight = cameraTransform.right;
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;
        float moveSpeed = isRunning ? runSpeed : walkSpeed;

        // Fall timer logic
        if (!isGrounded)
            fallTimer += Time.deltaTime;
        else
            fallTimer = 0f;

        bool isActuallyFalling = !isGrounded && fallTimer > fallAnimationThreshold;

        // Movement
        if (hasInput)
        {
            if (!hasMovementDelayPassed)
                StartCoroutine(StartMoveDelay());

            if (hasMovementDelayPassed)
            {
                controller.Move(moveDir * moveSpeed * Time.deltaTime);

                Quaternion targetRot = Quaternion.LookRotation(moveDir);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            }
        }
        else
        {
            hasMovementDelayPassed = false;
        }

        // Animation
        if (isActuallyFalling)
        {
            animator.SetFloat("Speed", 0f, 0.15f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", hasInput ? (isRunning ? 2f : 1f) : 0f, 0.15f, Time.deltaTime);
        }

        // Jump + Gravity
        if (isGrounded && !isJumping)
        {
            velocity.y = -2f;

            if (jumpRequested)
            {
                StartCoroutine(HandleJump());
                jumpRequested = false;
            }
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    IEnumerator StartMoveDelay()
    {
        yield return new WaitForSeconds(0.2f);
        hasMovementDelayPassed = true;
    }

    IEnumerator HandleJump()
    {
        isJumping = true;
        animator.SetBool("isJumping", true); // Force jump animation

        float speedParam = animator.GetFloat("Speed");
        if (speedParam < 0.1f)
            animator.SetInteger("jumpType", 0);
        else if (speedParam < 1.5f)
            animator.SetInteger("jumpType", 1);
        else
            animator.SetInteger("jumpType", 2);

        animator.SetTrigger("Jump");

        yield return new WaitForSeconds(0.23f); // let jump animation start

        velocity.y = Mathf.Sqrt(jumpHeight * Mathf.Abs(gravity));

        yield return new WaitUntil(() => controller.isGrounded);

        animator.SetBool("isJumping", false); // now allow transition back
        isJumping = false;
    }

}