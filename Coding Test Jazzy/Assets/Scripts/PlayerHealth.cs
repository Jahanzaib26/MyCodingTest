using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;

public class PlayerHealth : NetworkBehaviour

{

    public PlayerMovementDualSwinging playermove;

    [SyncVar(hook = nameof(OnDeadChanged))]
    public bool isDead = false;

    public InventoryManager inventoryManager;

    public GameObject Health;
    public GameObject Stamina;



    public float maxHealth = 100f;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth;
    public Image healthBar;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
        if (Health == null)
            Health = GameObject.Find("Health");
        if (Stamina == null)
            Stamina = GameObject.Find("Stamina");
    }

    void OnHealthChanged(float oldValue, float newValue)
    {
        UpdateBar();
    }

    [Server]
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        if (currentHealth < 0) currentHealth = 0;

        if (currentHealth == 0)
        {
            Die();
        }

        Debug.Log("🔥 Player took damage! Health: " + currentHealth);
    }


    void UpdateBar()
    {
        if (healthBar != null)
            healthBar.fillAmount = currentHealth / maxHealth;
    }

    [Server]
public void Die()
{
    if (isDead) return;

    isDead = true;
    

        if (inventoryManager == null)
    {
            Debug.Log("Inventory is Null");
            return;
            //inventoryManager.ServerClearInventoryOnDeath(connectionToClient);
    }
        inventoryManager.ServerClearInventoryOnDeath(connectionToClient);
        RpcUpdateUI(isDead);
    }
    //[ClientRpc]
    //void RpcOnDeath()
    //{
    //    transform.position = new Vector3(149f, 87f, -9f);
    //}
    [ClientRpc]
    void RpcUpdateUI(bool isDead)
    {
        // Safety check
        if (Health == null || Stamina == null)
            return;

        Health.SetActive(!isDead);
        Stamina.SetActive(!isDead);
    }

    void OnDeadChanged(bool oldValue, bool newValue)
    {
        if (!isLocalPlayer) return;

        Debug.Log($"💀 OnDeadChanged | newValue = {newValue}");

        // SAFETY CHECK
        if (Health == null || Stamina == null)
        {
            Debug.LogError("❌ Health or Stamina reference is NULL");
            return;
        }

        if (newValue) // DEAD
        {
            Debug.Log("🔴 PLAYER DEAD → UI OFF");

         

            MoveCamera camFollow = GetComponentInChildren<MoveCamera>();
            if (camFollow == null) return;

            Transform alivePlayer = FindAlivePlayer();
            if (alivePlayer != null)
            {
                camFollow.SetTarget(alivePlayer);
            }
            else
            {
                playermove.showfailpannel();
            }
        }
        else // REVIVED
        {
            Debug.Log("🟢 PLAYER REVIVED → UI ON");

        

            MoveCamera camFollow = GetComponentInChildren<MoveCamera>();
            if (camFollow != null)
                camFollow.SetTarget(transform);
        }
    }




    Transform FindAlivePlayer()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth ph in players)
        {
            if (!ph.isDead)
                return ph.transform;
        }

        return null;
    }
    //[Server]
    //public void Revive()
    //{
    //    isDead = false;
    //    currentHealth = maxHealth;

    //    RpcOnRevive();
    //}

    //[ClientRpc]
    //void RpcOnRevive()
    //{
    //    UpdateBar();
    //    Debug.Log("✨ Player Revived");
    //}
    [Command]
    public void CmdRequestReviveOther()
    {
        // Ye function SERVER pe chalega
        PlayerHealth deadPlayer = FindDeadPlayer();

        if (deadPlayer == null)
        {
            Debug.Log("❌ Server: No dead player found to revive");
            return;
        }

        Vector3 revivePosition = transform.position;

        deadPlayer.ServerRevive(revivePosition);
    }

    [Server]
    void ServerRevive(Vector3 revivePosition)
    {
        isDead = false;
        currentHealth = maxHealth;
        //Health.SetActive(true);
        //Stamina.SetActive(true);// 💯 health reset (server)

        RpcOnRevive(revivePosition);
        RpcUpdateUI(isDead); // show UI again
    }
    [ClientRpc]
    void RpcOnRevive(Vector3 revivePosition)
    {
        transform.position = revivePosition;
    }
    [Server]
    PlayerHealth FindDeadPlayer()
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
