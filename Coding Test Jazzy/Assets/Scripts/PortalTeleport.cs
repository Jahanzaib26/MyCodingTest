using Mirror;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;

public class PortalTeleportMirror : MonoBehaviour
{
    public Transform teleportPoint;

    private void OnTriggerEnter(Collider other)
    {
        // Sirf server par teleport hoga
        if (!NetworkServer.active)
            return;

        if (other.CompareTag("Player"))
        {
                other.transform.position = teleportPoint.position;
                other.transform.rotation = teleportPoint.rotation;    
        }
    }
}
