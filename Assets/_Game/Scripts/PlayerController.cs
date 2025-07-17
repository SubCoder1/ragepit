using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpHeight = 2.5f;
    public float gravity = -20f;
    public float rotationSpeed = 10f;
    public float jumpDelay = 0.15f; // delay before applying upward force

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

        isGrounded = controller.isGrounded;

        inputMovement = moveAction.action.ReadValue<Vector2>();
        isRunning = runAction.action.IsPressed();

        Vector3 inputDir = new Vector3(inputMovement.x, 0f, inputMovement.y);

        if (inputDir.sqrMagnitude > 0.01f)
        {
            Vector3 camForward = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camForward.y = 0f;
            camRight.y = 0f;
            camForward.Normalize();
            camRight.Normalize();

            Vector3 moveDir = camForward * inputDir.z + camRight * inputDir.x;
            float moveSpeed = isRunning ? runSpeed : walkSpeed;

            controller.Move(moveDir * moveSpeed * Time.deltaTime);

            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);

            animator.SetFloat("Speed", isRunning ? 2f : 1f, 0.2f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.2f, Time.deltaTime);
        }

        // Handle jumping
        if (isGrounded && !isJumping)
        {
            velocity.y = -1f;

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

    IEnumerator HandleJump()
    {
        isJumping = true;

        // Set the jump type (for animation logic)
        float speedParam = animator.GetFloat("Speed");

        if (speedParam < 0.1f)
            animator.SetInteger("jumpType", 0); // idle jump
        else if (speedParam < 1.5f)
            animator.SetInteger("jumpType", 1); // walk jump
        else
            animator.SetInteger("jumpType", 2); // run jump

        animator.SetTrigger("Jump");

        // ⏳ Wait for animation to visually blend before jumping
        yield return new WaitForSeconds(0.2f); // matches Animator transition time

        // Apply upward velocity
        velocity.y = Mathf.Sqrt(jumpHeight * Mathf.Abs(gravity));

        // Wait until grounded again to allow next jump
        yield return new WaitUntil(() => controller.isGrounded);
        isJumping = false;
    }

}
