using Mirror;
using UnityEngine;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;



    private void Start()
    {
        teleportPoint = GameObject.Find("teleportPoint").transform;
    }
    //private void OnTriggerEnter(Collider other)
    //{
    //    // 🔍 Ye client + server dono par fire hota hai
    //    Debug.Log(
    //      $"Trigger hit | ServerActive={NetworkServer.active} | ClientActive={NetworkClient.active}"
    //  );


    //    // ❗ Teleport sirf server karega
    //    //if (!NetworkServer.active) return;

    //    if (!other.CompareTag("Player")) return;

    //    // NetworkIdentity parent se lo
    //    NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
    //    if (ni == null) return;

    //    // ROOT (jahan NetworkIdentity + NetworkTransform hai)
    //    Transform playerRoot = ni.transform.GetChild(0).transform;

    //    // Child Rigidbody reset (optional but recommended)
    //    Rigidbody rb = other.GetComponent<Rigidbody>();
    //    if (rb != null)
    //    {
    //        rb.velocity = Vector3.zero;
    //        rb.angularVelocity = Vector3.zero;
    //    }

    //    // 🚀 SERVER TELEPORT
    //    playerRoot.SetPositionAndRotation(
    //        teleportPoint.position,
    //        teleportPoint.rotation
    //    );
    //}

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // Player ka NetworkIdentity (parent se)
        NetworkIdentity ni = other.GetComponentInParent<NetworkIdentity>();
        if (ni == null) return;

        // ❗ Sirf local player server ko request bheje
        if (!ni.isLocalPlayer) return;


        // ✅ Local teleport (instant, reliable)
        transform.SetPositionAndRotation(
            teleportPoint.position,
            teleportPoint.rotation
        );

        CmdSyncTeleport(teleportPoint.position,teleportPoint.rotation);
    }

    [Command]
    void CmdSyncTeleport(Vector3 pos, Quaternion rot)
    {
        transform.SetPositionAndRotation(pos, rot);
    }
}
