using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    [Header("References")]
    public Transform player;
    public Transform cameraTransform;
    public LookTouchZone lookTouchZone;
    public InputActionReference lookAction;
    public InputActionReference moveAction;

    [Header("Look Sensitivity")]
    public float joystickYawSpeed = 5000f;
    public float joystickPitchSpeed = 4440f;
    public float mouseYawSpeed = 120f;
    public float mousePitchSpeed = 100f;
    public bool useJoystickOnlyOnMobile = true;

    [Header("Touch Sensitivity Multiplier")]
    public float touchSensitivity = 15000f;

    [Header("Look Limits")]
    public float minPitch = -25f;
    public float maxPitch = 35f;

    [Header("Camera Offset")]
    public Vector3 offset = new Vector3(1.9f, 2.5f, -3f);

    [Header("Head Height")]
    public float headHeight = 1.9f;

    [Header("Collision")]
    public LayerMask collisionMask;
    public float sphereRadius = 0.3f;
    public float smoothSpeed = 10f;
    public float minCameraDistance = 1f;

    private float yaw = 0f;
    private float pitch = 5f;
    private float smoothedYaw = 0f;
    private float smoothedPitch = 5f;
    private Vector3 camVelocity;

    public float SmoothedYaw => smoothedYaw;

    private void OnEnable()
    {
        lookAction.action.Enable();
        moveAction.action.Enable();
    }

    private void OnDisable()
    {
        lookAction.action.Disable();
        moveAction.action.Disable();
    }

    private void LateUpdate()
    {
        if (player == null || cameraTransform == null) return;

        float dt = Mathf.Min(Time.deltaTime, 0.033f);

        // --- 1. Handle Look Input ---
        if (!useJoystickOnlyOnMobile || Application.isMobilePlatform)
        {
            Vector2 dragInput = lookTouchZone.IsDragging ? lookTouchZone.lookDelta : Vector2.zero;

            if (dragInput.sqrMagnitude > 0.001f)
            {
                yaw += dragInput.x * touchSensitivity;
                pitch -= dragInput.y * touchSensitivity;
            }
        }

#if UNITY_EDITOR || UNITY_STANDALONE
        // --- 2. Mouse input for desktop ---
        Vector2 mouseInput = lookAction.action.ReadValue<Vector2>();
        if (mouseInput.sqrMagnitude > 0.01f)
        {
            yaw += mouseInput.x * mouseYawSpeed * dt;
            pitch += mouseInput.y * mousePitchSpeed * dt;
        }
#endif

        // --- 3. Clamp pitch ---
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // --- 4. Smooth Rotation ---
        smoothedYaw = yaw;
        smoothedPitch = pitch;

        // --- 5. Set Camera Pivot ---
        transform.SetPositionAndRotation(player.position, Quaternion.Euler(smoothedPitch, smoothedYaw, 0f));

        // --- 6. Desired Camera Position ---
        Vector3 pivot = transform.position;
        Vector3 desiredWorldPos = transform.TransformPoint(offset);
        Vector3 camDir = (desiredWorldPos - pivot).normalized;
        float desiredDistance = offset.magnitude;

        Vector3 finalPos = desiredWorldPos;

        // --- 7. Obstacle Avoidance (start from head height) ---
        Vector3 headPivot = pivot + Vector3.up * (headHeight + 0.5f);

        if (Physics.SphereCast(headPivot, sphereRadius, camDir, out RaycastHit hit, desiredDistance, collisionMask))
        {
            float hitDist = Mathf.Max(hit.distance - 0.05f, minCameraDistance);
            finalPos = headPivot + camDir * hitDist;
        }

        for (int i = 0; i < 10 && Physics.CheckSphere(finalPos, sphereRadius, collisionMask); i++)
        {
            finalPos -= camDir * 0.05f;
        }

        // --- 8. Smooth Camera Movement ---
        cameraTransform.position = Vector3.SmoothDamp(cameraTransform.position, finalPos, ref camVelocity, dt * smoothSpeed);
        cameraTransform.rotation = Quaternion.Euler(smoothedPitch, smoothedYaw, 0f);
    }
}
