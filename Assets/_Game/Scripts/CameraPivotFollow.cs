using UnityEngine;

public class CameraPivotFollow : MonoBehaviour
{
    public Transform player;
    public Vector3 pivotOffset = new Vector3(0, 1.6f, 0);
    public float followSpeed = 10f;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition = player.position + pivotOffset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }
}
