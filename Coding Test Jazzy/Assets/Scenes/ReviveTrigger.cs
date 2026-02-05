using UnityEngine;
using Mirror;

public class ReviveTrigger : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("🔥 Trigger ENTER detected (client or server)");

        if (!isServer)
        {
            Debug.Log("❌ Not server, exiting");
            return;
        }

        Debug.Log("✅ Trigger ENTER on SERVER");

        PlayerHealth reviver = other.GetComponent<PlayerHealth>();
        if (reviver == null)
        {
            Debug.Log("❌ No PlayerHealth on entering object");
            return;
        }

        Debug.Log($"🟡 Reviver found | netId={reviver.netId} | isDead={reviver.isDead}");

        if (reviver.isDead)
        {
            Debug.Log("❌ Reviver is dead, cannot revive");
            return;
        }

        PlayerHealth dead = FindAnyDeadPlayer();
        if (dead == null)
        {
            Debug.Log("❌ No dead player found on server");
            return;
        }

        Debug.Log($"🟢 Reviving player {dead.netId}");

        dead.Revive();
    }


    PlayerHealth FindAnyDeadPlayer()
    {

        foreach (NetworkIdentity ni in NetworkServer.spawned.Values)
        {
            PlayerHealth ph = ni.GetComponent<PlayerHealth>();
            if (ph == null) continue;

            Debug.Log($"🔍 Checking player {ph.netId} | isDead={ph.isDead}");

            if (ph.isDead)
                return ph;
        }


        foreach (NetworkIdentity ni in NetworkServer.spawned.Values)
        {
            PlayerHealth ph = ni.GetComponent<PlayerHealth>();
            if (ph != null && ph.isDead)
                return ph;
        }
        return null;
    }
}
