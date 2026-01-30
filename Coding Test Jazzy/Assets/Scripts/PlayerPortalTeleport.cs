using Mirror;
using Mirror.Examples.AdditiveLevels;
using UnityEngine;

public class PlayerPortalTeleport : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isLocalPlayer) return;

        Portal portal = other.GetComponent<Portal>();
        if (portal == null) return;

        CmdTeleport(portal.teleportPoint.position, portal.teleportPoint.rotation);
    }

    [Command]     
    void CmdTeleport(Vector3 pos, Quaternion rot)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.position = pos;
        rb.rotation = rot;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}
