using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class ChotuController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveRadius = 10f;
    public float waitTime = 2f;
    public float wanderSpeed = 2.5f;
    public float approachSpeed = 1.5f;

    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float nearRange = 3f;

    [Header("VFX Settings")]
    public GameObject vfxEffect;
    public float vfxDelay = 5f;
    public float damageRange = 4f;
    public float damagePerSecond = 15f;

    private NavMeshAgent agent;
    
    [SerializeField] private Transform playerTarget;
    [SerializeField] private PlayerHealth playerHealth;

    private bool playerDetected = false;
    private bool vfxTriggered = false;

    private Coroutine followRoutine;
    private Coroutine vfxRoutine;
    private Coroutine damageRoutine;

    [Header("Audio Settings")]
    public AudioSource walkAudioSource;
    public AudioClip walkClip;
    public float minVolume = 0.05f;
    public float maxVolume = 1f;
    public float maxHearingDistance = 15f;

    [Header("Fire Audio Settings")]
    public AudioSource fireSound;
    public AudioClip fireClip;
    public float maxFireHearingDistance = 12f;

    // ✅ Animation System  
    [Header("Animation Settings")]
    public Animator animator;


    [Header("Material Settings")]
    public Renderer enemyRenderer;
    public Material normalMaterial;
    public Material fireMaterial;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer.material = normalMaterial;

        if (!isServer) return; // 🚨 server only

        playerTarget = null;
        playerHealth = null;

        if (vfxEffect != null)
            vfxEffect.SetActive(false);

        SetNewRandomDestination();
    }

    void Update()
    {
        if (!isServer) return; // 🚨 SERVER ONLY AI
        HandlePlayerDetection();

        // 🟢 Animation Speed Control
        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        if (!playerDetected)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                SetNewRandomDestination();

            FaceMovementDirection();
        }

        if (playerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            HandleFootstepAudio(distance);
            HandleFireAudio(distance);
        }
    }

    void HandlePlayerDetection()
    {
        if (!isServer) return;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDistance = Mathf.Infinity;
        Transform closestPlayer = null;
        PlayerHealth closestHealth = null;

        foreach (GameObject p in players)
        {
            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist <= detectionRange && dist < closestDistance)
            {
                closestDistance = dist;
                closestPlayer = p.transform;
                closestHealth = p.GetComponent<PlayerHealth>();
            }
        }

        if (closestPlayer != null && !playerDetected)
        {
            playerTarget = closestPlayer;
            playerHealth = closestHealth;
            playerDetected = true;
            if (followRoutine != null) StopCoroutine(followRoutine);
            followRoutine = StartCoroutine(FollowPlayerDynamically());
        }
        else if (closestPlayer == null && playerDetected)
        {
            ResetState();
        }

    }

    public void ResetState() {

        playerDetected = false;

        // RESET MATERIAL
        if (enemyRenderer != null && normalMaterial != null)
            enemyRenderer.material = normalMaterial;

        // STOP VFX
        if (vfxEffect != null)
            vfxEffect.SetActive(false);

        // STOP WALKING AUDIO
        if (walkAudioSource != null && walkAudioSource.isPlaying)
            walkAudioSource.Stop();

        // STOP FIRE AUDIO
        if (fireSound != null && fireSound.isPlaying)
            fireSound.Stop();

        vfxTriggered = false;

        if (vfxRoutine != null)
        {
            StopCoroutine(vfxRoutine);
            vfxRoutine = null;
        }

        if (damageRoutine != null)
        {
            StopCoroutine(damageRoutine);
            damageRoutine = null;
        }

        if (followRoutine != null)
            StopCoroutine(followRoutine);

        playerTarget = null;
        playerHealth = null;

        agent.isStopped = false;
        SetNewRandomDestination();
    }

    IEnumerator FollowPlayerDynamically()
    {
        while (playerDetected && playerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);
            float engageRange = nearRange * 0.9f;  // <— yahan lagayega
            if (distance > nearRange)
            {
                // Move toward player
                agent.isStopped = false;
                agent.speed = approachSpeed;

                Vector3 dir = (playerTarget.position - transform.position).normalized;
                Vector3 stopPos = playerTarget.position - dir * nearRange;
                agent.SetDestination(stopPos);
            }
            else if (distance <= engageRange)
            {
                // Full STOP - keep stable
                agent.isStopped = true;
                FacePlayer();

                if (!vfxTriggered)
                {
                    vfxTriggered = true;
                    StartCoroutine(SmoothMaterialTransition());
                    vfxRoutine = StartCoroutine(StartVFXAfterDelay());
                }
            }
            else
            {
                agent.isStopped = true;
                FacePlayer();

                if (!vfxTriggered)
                {
                    vfxTriggered = true;

                    // 🟢 Smooth MATERIAL TRANSITION first
                    RpcStartMaterialTransition();


                    // 🟢 Then fire VFX trigger delay
                    vfxRoutine = StartCoroutine(StartVFXAfterDelay());
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        agent.isStopped = false;
        SetNewRandomDestination();
    }

    [ClientRpc]
    void RpcStartMaterialTransition()
    {
        StartCoroutine(SmoothMaterialTransition());
    }

    IEnumerator SmoothMaterialTransition()
    {
        if (enemyRenderer == null || fireMaterial == null || normalMaterial == null)
            yield break;

        float duration = 1f;
        float time = 0f;

        Material tempMat = new Material(normalMaterial);

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;

            tempMat.Lerp(normalMaterial, fireMaterial, t);
            enemyRenderer.material = tempMat;

            yield return null;
        }

        enemyRenderer.material = fireMaterial;
    }




    IEnumerator StartVFXAfterDelay()
    {
        yield return new WaitForSeconds(vfxDelay);

        if (!playerDetected) yield break;

        RpcPlayVFXAndFire();   // 👈 CLIENTS

        if (damageRoutine == null)
            damageRoutine = StartCoroutine(ApplyDamageWhileVFXActive());
    }
    [ClientRpc]
    void RpcPlayVFXAndFire()
    {
        if (vfxEffect != null)
            vfxEffect.SetActive(true);

        if (fireSound != null && fireClip != null)
        {
            fireSound.clip = fireClip;
            fireSound.loop = true;
            fireSound.Play();
        }

        Debug.Log("✨ VFX & Fire started (CLIENT)");
    }



    IEnumerator ApplyDamageWhileVFXActive()
    {
        while (vfxEffect != null && vfxEffect.activeSelf && playerDetected)
        {
            if (playerTarget == null || playerHealth == null) yield break;

            float distance = Vector3.Distance(transform.position, playerTarget.position);

            if (distance <= damageRange)
            {
                if (isServer && playerHealth != null)
                {
                    if (isServer)
                    {
                        playerHealth.TakeDamage(damagePerSecond);
                        Debug.Log("🔥 Damage applied: " + damagePerSecond);// or instant kill
                    }
                    //playerHealth.TakeDamage(damagePerSecond);
                    //Debug.Log("🔥 Damage applied: " + damagePerSecond);
                }
            }

            yield return new WaitForSeconds(1f);
        }

        damageRoutine = null;
    }

    void HandleFootstepAudio(float playerDistance)
    {
        if (walkAudioSource == null || walkClip == null) return;

        bool isWalking = agent.velocity.magnitude > 0.1f && !agent.isStopped;

        if (isWalking && !walkAudioSource.isPlaying)
        {
            walkAudioSource.clip = walkClip;
            walkAudioSource.loop = true;
            walkAudioSource.Play();
        }
        else if (!isWalking && walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }

        if (playerDistance <= maxHearingDistance)
        {
            float t = 1f - (playerDistance / maxHearingDistance);
            walkAudioSource.volume = Mathf.Lerp(minVolume, maxVolume, t);
        }
        else
        {
            walkAudioSource.volume = 0f;
        }
    }

    void HandleFireAudio(float playerDistance)
    {
        if (fireSound == null || !fireSound.isPlaying) return;

        if (playerDistance <= maxFireHearingDistance)
        {
            float t = 1f - (playerDistance / maxFireHearingDistance);
            fireSound.volume = Mathf.Lerp(minVolume, maxVolume, t);
        }
        else
        {
            fireSound.volume = 0f;
        }
    }

    void FacePlayer()
    {
        if (playerTarget == null) return;

        Vector3 lookDir = playerTarget.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            Quaternion lookRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 4f);
        }
    }

    void SetNewRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius + transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
        {
            agent.speed = wanderSpeed;
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    void FaceMovementDirection()
    {
        if (agent.velocity.sqrMagnitude < 0.01f) return;

        Vector3 moveDir = agent.velocity.normalized;
        moveDir.y = 0;

        if (moveDir != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }
}
