using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System.Collections;

public class ReviveTrigger : NetworkBehaviour
{
    private PlayerHealth aliveLocalPlayer;
    public GameObject myCanvas;
    public Text money;
    public Text NotEnoughMoney;
    public int reviveCost = 2000;


    void OnTriggerEnter(Collider other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();

        if (ph == null) return;
        if (!ph.isLocalPlayer) return;
        if (ph.isDead) return;   // ❗ ONLY ALIVE player

        Debug.Log("🟢 Alive player entered revive trigger");

        // 👇 Set default money value
        money.text = "2000$";

        myCanvas.SetActive(true);

        aliveLocalPlayer = ph;
    }

    void OnTriggerExit(Collider other)
    {
        if (aliveLocalPlayer == null) return;

        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph != aliveLocalPlayer) return;

        Debug.Log("🔴 Alive player left revive trigger");
        myCanvas.SetActive(false);

        aliveLocalPlayer = null;
    }

    void Update()
    {
        if (aliveLocalPlayer == null) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("R Pressed");

            InventoryManager inv = aliveLocalPlayer.GetComponentInChildren<InventoryManager>(true);

            if (inv == null)
            {
                Debug.LogError("InventoryManager not found!");
                return;
            }

            int playerMoney = inv.GetTotalPrice();
            Debug.Log("Player Money: " + playerMoney);

            if (playerMoney < reviveCost)
            {
                Debug.Log("❌ Not enough money for revive");
                StartCoroutine(ShowMessageForSeconds("Not Enough Money!"));
                return;
            }


            // 🔎 Check if any dead player exists
            PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();
            PlayerHealth deadPlayer = null;

            foreach (PlayerHealth ph in players)
            {
                if (ph.isDead)
                {
                    deadPlayer = ph;
                    break;
                }
            }

            if (deadPlayer == null)
            {
                Debug.Log("⚠ No dead players found");
                StartCoroutine(ShowMessageForSeconds("No Dead Players Found!"));
                return;
            }


            // ✅ Now revive exists → do money logic

            inv.DeductMoney(reviveCost);

            if (TotalCollectManager.Instance != null)
            {
                TotalCollectManager.Instance.Add(reviveCost);
            }

            myCanvas.SetActive(false);

            Debug.Log("🟢 Revive triggered on dead player");

            aliveLocalPlayer.CmdRequestReviveOther();

            aliveLocalPlayer = null;
        }
    }

    IEnumerator ShowMessageForSeconds(string message)
    {
        NotEnoughMoney.text = message;
        NotEnoughMoney.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);

        NotEnoughMoney.gameObject.SetActive(false);
    }




}
