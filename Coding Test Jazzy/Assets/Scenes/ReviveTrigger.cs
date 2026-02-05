using UnityEngine;
using Mirror;

public class ReviveTrigger : NetworkBehaviour
{
    bool used = false;

    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;
        if (used) return;

        PlayerHealth reviver = other.GetComponent<PlayerHealth>();
        if (reviver == null) return;
        if (reviver.isDead) return;

        PlayerHealth dead = FindAnyDeadPlayer();
        if (dead == null) return;

        used = true;
        dead.Revive();
    }

    PlayerHealth FindAnyDeadPlayer()
    {
        foreach (NetworkIdentity ni in NetworkServer.spawned.Values)
        {
            PlayerHealth ph = ni.GetComponent<PlayerHealth>();
            if (ph != null && ph.isDead)
                return ph;
        }
        return null;
    }
}
