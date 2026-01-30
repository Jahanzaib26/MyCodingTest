using UnityEngine;
using Mirror;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkIdentity ni = other.GetComponent<NetworkIdentity>();
        if (ni == null) return;

        // HOST (server detects + acts)
        if (isServer)
        {
            Teleport(other.attachedRigidbody);
        }
        // CLIENT detects → asks server
        else if (ni.isLocalPlayer)
        {
            CmdRequestTeleport(ni);
        }
    }

    [Command]
    private void CmdRequestTeleport(NetworkIdentity playerId)
    {
        Rigidbody rb = playerId.GetComponent<Rigidbody>();
        if (rb == null) return;

        Teleport(rb);
    }

    [Server]
    private void Teleport(Rigidbody rb)
    {
        rb.position = teleportPoint.position;
        rb.rotation = teleportPoint.rotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
