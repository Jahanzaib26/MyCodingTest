using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Offset")]
    public Vector3 offset = new Vector3(0f, 8f, -12f);

    [Header("Follow Settings")]
    public float followSmoothness = 10f;

    void LateUpdate()
    {
        if (!target) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x,
            offset.y,
            target.position.z + offset.z
        );

        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            Time.deltaTime * followSmoothness
        );
    }
}
