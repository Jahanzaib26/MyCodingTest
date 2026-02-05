using UnityEngine;
using Mirror;

public class ReviveTrigger : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        PlayerHealth reviver = other.GetComponent<PlayerHealth>();
        if (reviver == null) return;

        // alive players only
        if (reviver.isDead) return;

        PlayerHealth dead = FindAnyDeadPlayer();
        if (dead == null) return;

        Debug.Log($"🟢 Reviving player {dead.netId}");

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
