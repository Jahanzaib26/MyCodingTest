using Mirror;
using UnityEngine;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        // ❗ Sirf server teleport karega
        if (!NetworkServer.active) return;

        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb == null) return;

        // Rigidbody-safe teleport
        rb.position = teleportPoint.position;
        rb.rotation = teleportPoint.rotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
