using UnityEngine;
using Mirror;

public class PortalSpawner : NetworkBehaviour
{
    public GameObject portalPrefab;
    public Transform[] spawnPoints; // array of positions

    public override void OnStartServer()
    {
        base.OnStartServer();
        SpawnPortals();
    }

    [Server]
    void SpawnPortals()
    {
        foreach (Transform point in spawnPoints)
        {
            GameObject portal = Instantiate(
                portalPrefab,
                point.position,
                point.rotation
            );

            NetworkServer.Spawn(portal);
        }
    }
}
