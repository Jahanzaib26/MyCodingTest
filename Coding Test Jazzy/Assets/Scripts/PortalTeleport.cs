using Mirror;
using UnityEngine;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        // ❗ Sirf server teleport kare
        if (!NetworkServer.active) return;

        if (!other.CompareTag("Player")) return;

        // Parent se NetworkIdentity lo
        NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
        if (ni == null) return;

        // Parent transform ko teleport karo
        Transform playerRoot = ni.transform;

        // Agar child par Rigidbody hai to usko reset karo
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        playerRoot.position = teleportPoint.position;
        playerRoot.rotation = teleportPoint.rotation;
    }
}
