using UnityEngine;
using UnityEngine.UI;

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

        if (newValue)
        {
            Debug.Log("☠️ LOCAL PLAYER DIED – client detected it");
        }
    }


}
