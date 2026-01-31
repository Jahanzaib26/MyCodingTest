using Mirror;
using UnityEngine;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        NetworkIdentity ni = other.GetComponent<NetworkIdentity>();
        if (ni == null) return;

        // Client → Server request
        CmdTeleportPlayer(ni.connectionToClient);
    }

    [Command(requiresAuthority = false)]
    void CmdTeleportPlayer(NetworkConnectionToClient conn)
    {
        if (conn == null || conn.identity == null) return;

        // Server teleport
        conn.identity.transform.SetPositionAndRotation(
            teleportPoint.position,
            teleportPoint.rotation
        );

        // Client ko bhi teleport karao
        TargetTeleport(conn, teleportPoint.position, teleportPoint.rotation);
    }

    // Sirf usi client ko call hota hai
    [TargetRpc]
    void TargetTeleport(NetworkConnection conn, Vector3 pos, Quaternion rot)
    {
        if (conn.identity == null) return;

        conn.identity.transform.SetPositionAndRotation(pos, rot);
    }
}
