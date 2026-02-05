using UnityEngine;
using UnityEngine.UI;
using System.Linq;

using Mirror;

public class PlayerHealth : NetworkBehaviour

{
    [SyncVar(hook = nameof(OnDeadChanged))]
    public bool isDead = false;



    public float maxHealth = 100f;
    [SyncVar(hook = nameof(OnHealthChanged))]
    public float currentHealth;
    public Image healthBar;

    public override void OnStartServer()
    {
        currentHealth = maxHealth;
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

        isDead = true;   // 🔴 THIS is the signal
        RpcOnDeath();
    }

    [ClientRpc]
    void RpcOnDeath()
    {
        transform.position = new Vector3(149f, 87f, -9f);
    }

    void OnDeadChanged(bool oldValue, bool newValue)
    {
        if (!isLocalPlayer) return;

        MoveCamera camFollow = GetComponentInChildren<MoveCamera>();

        if (camFollow == null)
        {
            Debug.LogError("❌ CameraFollow not found");
            return;
        }

        if (newValue) // DEAD
        {
            Debug.Log("👁 Switching camera to alive player");

            Transform alivePlayer = FindAlivePlayer();
            if (alivePlayer != null)
                camFollow.SetTarget(alivePlayer);
        }
        else // 🔥 REVIVED
        {
            Debug.Log("✨ Camera returned to revived player");
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
        currentHealth = maxHealth; // 💯 health reset (server)

        RpcOnRevive(revivePosition);
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
