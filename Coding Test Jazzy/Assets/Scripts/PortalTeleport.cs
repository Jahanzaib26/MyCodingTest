using Mirror;
using UnityEngine;

public class PortalTeleportMirror : NetworkBehaviour
{
    public Transform teleportPoint;



    private void Start()
    {
        teleportPoint = GameObject.Find("teleportPoint").transform;
    }
    private void OnTriggerEnter(Collider other)
    {
        // 🔍 Ye client + server dono par fire hota hai
        Debug.Log(
          $"Trigger hit | ServerActive={NetworkServer.active} | ClientActive={NetworkClient.active}"
      );


        // ❗ Teleport sirf server karega
        //if (!NetworkServer.active) return;

        if (!other.CompareTag("Player")) return;

        // NetworkIdentity parent se lo
        NetworkIdentity ni = other.GetComponent<NetworkIdentity>();
        if (ni == null) return;

        // ROOT (jahan NetworkIdentity + NetworkTransform hai)
        Transform playerRoot = ni.transform.GetChild(0).transform;

        // Child Rigidbody reset (optional but recommended)
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // 🚀 SERVER TELEPORT
        playerRoot.SetPositionAndRotation(
            teleportPoint.position,
            teleportPoint.rotation
        );
    }
}



