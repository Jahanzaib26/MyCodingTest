using Mirror;
using UnityEngine;

public class BackPortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;



    private void Start()
    {
        teleportPoint = GameObject.Find("backteleportpoint").transform;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Player ka NetworkIdentity (parent se)
        NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
        if (ni == null) return;

        // ❗ Sirf local player server ko request bheje
        if (!ni.isLocalPlayer) return;


        // ✅ Local teleport (instant, reliable)
        other.transform.SetPositionAndRotation(
            teleportPoint.position,
            teleportPoint.rotation
        );

        CmdSyncTeleport(teleportPoint.position, teleportPoint.rotation, other.transform);
    }

    [Command]
    void CmdSyncTeleport(Vector3 pos, Quaternion rot, Transform other)
    {
        other.SetPositionAndRotation(pos, rot);
    }



}
