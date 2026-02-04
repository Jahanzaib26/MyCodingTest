using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class ReviveTrigger : NetworkBehaviour
{
    //public Text reviveText;

    private PlayerHealth deadPlayer;
    private bool canRevive = false;

    void Start()
    {
        //if (reviveText != null)
        //    reviveText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!canRevive) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            CmdRequestRevive(deadPlayer.netIdentity);
           // reviveText.gameObject.SetActive(false);
            canRevive = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("🟡 Trigger ENTER detected by: " + other.name);
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null || !ph.isDead) return;

        if (!other.GetComponent<NetworkBehaviour>().isLocalPlayer) return;

        deadPlayer = ph;
        canRevive = true;
        Debug.Log(
          "✅ PlayerHealth FOUND | " +
          "isLocalPlayer = " + ph.isLocalPlayer +
          " | isServer = " + isServer +
          " | isClient = " + isClient +
          " | isDead = " + ph.isDead
      );
        //if (reviveText != null)
        //    reviveText.gameObject.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != deadPlayer) return;

        canRevive = false;
        deadPlayer = null;

        //if (reviveText != null)
        //    reviveText.gameObject.SetActive(false);
    }

    // 🔴 CLIENT → SERVER
    [Command]
    void CmdRequestRevive(NetworkIdentity deadPlayerNetId)
    {
        PlayerHealth ph = deadPlayerNetId.GetComponent<PlayerHealth>();
        if (ph != null && ph.isDead)
        {
            ph.Revive();
        }
    }
}
