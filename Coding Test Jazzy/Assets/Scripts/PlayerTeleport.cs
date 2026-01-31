using UnityEngine;
using Mirror;

public class PlayerTeleport : NetworkBehaviour
{
    private bool teleporting = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        PortalTrigger portal = other.GetComponent<PortalTrigger>();
        if (portal == null) return;

        if (teleporting) return;
        teleporting = true;

        Debug.Log("LOCAL PLAYER TELEPORTED");

        // 1️⃣ Move locally (required for CharacterController)
        transform.SetPositionAndRotation(
            portal.teleportPoint.position,
            portal.teleportPoint.rotation
        );

        // 2️⃣ Sync to server
        CmdSyncTeleport(portal.teleportPoint.position, portal.teleportPoint.rotation);

        Invoke(nameof(ResetTeleport), 0.5f);
    }

    void ResetTeleport()
    {
        teleporting = false;
    }

    [Command]
    void CmdSyncTeleport(Vector3 pos, Quaternion rot)
    {
        // Server updates authoritative state
        transform.SetPositionAndRotation(pos, rot);
    }
}
