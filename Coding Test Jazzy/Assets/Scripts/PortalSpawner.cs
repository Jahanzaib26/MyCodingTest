using UnityEngine;
using Mirror;

public class PortalSpawner : NetworkBehaviour
{
    public GameObject portalPrefab;
    public Transform spawnPoint;

    public override void OnStartServer()
    {
        base.OnStartServer();

        SpawnPortal();
    }

    [Server]
    void SpawnPortal()
    {
        GameObject portal = Instantiate(
            portalPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        NetworkServer.Spawn(portal);
    }
}
