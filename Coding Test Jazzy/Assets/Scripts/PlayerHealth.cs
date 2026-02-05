using Mirror;
using Mirror.Examples.Benchmark;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    void Update()
    {
        if (!isLocalPlayer) return;

        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    CmdCheckIfDeadPlayerExists();
        //}


        //if (Input.GetKeyDown(KeyCode.R))
        //{
        //    CmdReviveAnyDeadPlayer();
        //}
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

    Vector3 GetSpawnPoint()
    {
        GameObject spawn = GameObject.FindGameObjectWithTag("SpawnPoint");
        if (spawn != null)
            return spawn.transform.position;

        // fallback
        return Vector3.zero;
    }


    void OnDeadChanged(bool oldValue, bool newValue)
    {
        //if (!isLocalPlayer) return;

        //if (newValue)
        //{
        //    Debug.Log("👁 Switching camera to alive player");

        //    Transform alivePlayer = FindAlivePlayer();

        //    if (alivePlayer == null)
        //    {
        //        Debug.Log("❌ No alive player found");
        //        return;
        //    }

        //    MoveCamera camFollow = GetComponentInChildren<MoveCamera>();

        //    if (camFollow != null)
        //    {
        //        camFollow.SetTarget(alivePlayer);
        //    }
        //    else
        //    {
        //        Debug.LogError("❌ CameraFollow not found");
        //    }
        //}
    }
    [Server]
    public static PlayerHealth GetAnyDeadPlayer()
    {
        PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

        foreach (PlayerHealth ph in players)
        {
            if (ph.isDead)
                return ph;
        }

        return null;
    }
    [Command]
    //public void CmdCheckIfDeadPlayerExists()
    //{
    //    PlayerHealth dead = GetAnyDeadPlayer();

    //    if (dead != null)
    //        Debug.Log($"🟥 DEAD PLAYER FOUND: {dead.netId}");
    //    else
    //        Debug.Log("🟢 No dead players on server");
    //}

    //Transform FindAlivePlayer()
    //{
    //    PlayerHealth[] players = FindObjectsOfType<PlayerHealth>();

    //    foreach (PlayerHealth ph in players)
    //    {
    //        if (!ph.isDead)
    //            return ph.transform;
    //    }

    //    return null;
    //}
    [Server]
    public void Revive()
    {
        if (!isDead) return;

        isDead = false;
        currentHealth = maxHealth;

        RpcOnRevived(); // 🔥 THIS was missing

        Debug.Log($"🟢 Player {netId} revived");
    }
    [ClientRpc]
    void RpcOnRevived()
    {
        // reset position (important!)
        transform.position = GetSpawnPoint();

        // re-enable components
        EnablePlayer(true);

        UpdateBar();

        Debug.Log("✨ Player revived on client");
    }
    void EnablePlayer(bool enable)
    {
        // movement
        var controller = GetComponent<CharacterController>();
        if (controller) controller.enabled = enable;

        var motor = GetComponent<PlayerMovement>(); // your movement script
        if (motor) motor.enabled = enable;

        // colliders
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = enable;
    }
    [ClientRpc]
    void RpcOnDeath()
    {
        EnablePlayer(false);
        transform.position = new Vector3(149f, 87f, -9f);
    }

    [Command]
    public void CmdReviveAnyDeadPlayer()
    {
        PlayerHealth dead = GetAnyDeadPlayer();

        if (dead == null)
        {
            Debug.Log("❌ No dead player to revive");
            return;
        }

        dead.Revive();
    }

    //[Command]
    //public void CmdReviveAnyDeadPlayer()
    //{
    //    PlayerHealth dead = GetAnyDeadPlayer();

    //    if (dead == null)
    //    {
    //        Debug.Log("❌ No dead player to revive");
    //        return;
    //    }

    //    dead.Revive();
    //}


    [ClientRpc]
    void RpcOnRevive()
    {
        UpdateBar();
    }

    //[Command]
    //public void CmdRequestReviveOther()
    //{
    //    if (isDead) return; // safety

    //    //PlayerHealth deadPlayer = FindDeadPlayer();

    //    //if (deadPlayer == null)
    //    //{
    //    //    Debug.Log("❌ No dead player found to revive");
    //    //    return;
    //    //}

    //    //deadPlayer.ReviveOnServer();
    //}


}
