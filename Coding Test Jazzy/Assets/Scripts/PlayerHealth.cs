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
        if (!newValue) return; // only when dead

        Debug.Log("👁 Switching camera to alive player");

        // Find my camera mover
        MoveCamera myCamera = GetComponentInChildren<MoveCamera>();
        if (myCamera == null)
        {
            Debug.LogError("❌ MoveCamera not found");
            return;
        }

        // Find all players
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth p in players)
        {
            // skip myself
            if (p == this) continue;

            // skip dead players
            if (p.currentHealth <= 0) continue;

            // get alive player's camera position
            MoveCamera aliveCam = p.GetComponentInChildren<MoveCamera>();
            if (aliveCam != null)
            {
                myCamera.SetTarget(aliveCam.cameraPosition);
                Debug.Log("✅ Camera now following alive player");
                break;
            }
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


}
