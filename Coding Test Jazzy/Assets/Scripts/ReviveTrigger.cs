using UnityEngine;
using Mirror;

public class ReviveTrigger : NetworkBehaviour
{
    private PlayerHealth aliveLocalPlayer;

    void OnTriggerEnter(Collider other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();

        if (ph == null) return;
        if (!ph.isLocalPlayer) return;
        if (ph.isDead) return;   // ❗ ONLY ALIVE player

        Debug.Log("🟢 Alive player entered revive trigger");

        aliveLocalPlayer = ph;
    }

    void OnTriggerExit(Collider other)
    {
        if (aliveLocalPlayer == null) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != aliveLocalPlayer) return;

        Debug.Log("🔴 Alive player left revive trigger");
        aliveLocalPlayer = null;
    }

    void Update()
    {
        if (aliveLocalPlayer == null) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("🟡 Alive player pressed R → request revive");
            aliveLocalPlayer.CmdRequestReviveOther();
            aliveLocalPlayer = null;
        }
    }
}
