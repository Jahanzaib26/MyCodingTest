using Mirror;
using UnityEngine;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
        if (ni == null) return;

        // ✅ Local player khud move kare
        if (ni.isLocalPlayer)
        {
            other.transform.SetPositionAndRotation(
                teleportPoint.position,
                teleportPoint.rotation
            );
        }

        // ✅ Server ko sirf inform karo (sync ke liye)
        CmdTeleportPlayer(ni.netId);
    }

    [Command(requiresAuthority = false)]
    void CmdTeleportPlayer(uint netId)
    {
        if (!NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity))
            return;

        identity.transform.SetPositionAndRotation(
            teleportPoint.position,
            teleportPoint.rotation
        );
    }
}
