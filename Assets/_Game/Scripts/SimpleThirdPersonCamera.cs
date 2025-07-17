using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCamera : MonoBehaviour
{
    public Transform player;
    public InputActionReference lookAction;

    public float yawSpeed = 120f;
    public float pitchSpeed = 100f;
    public float minPitch = -10f;
    public float maxPitch = 10f;

    private float yaw = 0f;
    private float pitch = 1f;

    private void OnEnable() => lookAction.action.Enable();
    private void OnDisable() => lookAction.action.Disable();

    void LateUpdate()
    {
       Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        yaw += lookInput.x * yawSpeed * Time.deltaTime;
        pitch += lookInput.y * pitchSpeed * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, -10f, 10f); // keeps it mostly flat

        Vector3 offset = new Vector3(1.8f, 2.2f, -5.2f);    // shoulder, lower angle, cam distance from player
        transform.position = player.position + Quaternion.Euler(0, yaw, 0) * offset;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }
}
