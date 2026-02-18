using Mirror;
using UnityEngine;

public class NetworkItem : NetworkBehaviour
{
    [SyncVar]
    private bool isPicked = false;

    public void Pick()
    {
        if (!isServer) return;

        if (isPicked) return;

        isPicked = true;
        NetworkServer.Destroy(gameObject);
    }
}
