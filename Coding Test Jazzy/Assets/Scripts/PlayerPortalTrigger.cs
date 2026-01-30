using UnityEngine;
using Mirror;

public class PlayerPortalTrigger : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        Debug.Log($"[SERVER] Trigger entered by {netIdentity.netId}");
        PortalTeleport portal = other.GetComponent<PortalTeleport>();
        if (portal == null) return;

        portal.ServerRequestTeleport(netIdentity);
    }
}
