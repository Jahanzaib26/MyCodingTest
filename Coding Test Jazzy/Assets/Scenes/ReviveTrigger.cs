using UnityEngine;
using Mirror;

public class ReviveTrigger : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!isServer) return;

        PlayerHealth reviver = other.GetComponent<PlayerHealth>();
        if (reviver == null) return;

        // ❌ dead players cannot revive
        if (reviver.isDead) return;

        PlayerHealth dead = FindAnyDeadPlayer();
        if (dead == null) return;

        dead.Revive();
    }

    PlayerHealth FindAnyDeadPlayer()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth ph in players)
        {
            if (ph.isDead)
                return ph;
        }

        return null;
    }
}
