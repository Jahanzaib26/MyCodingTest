using System.Collections;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class ChotuController : NetworkBehaviour
{
    [SyncVar]
    private bool isBurningPlayer = false;

    [Header("Movement Settings")]
    public float moveRadius = 10f;
    public float wanderSpeed = 2.5f;
    public float approachSpeed = 1.5f;

    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float nearRange = 3f;

    [Header("VFX & Damage")]
    public GameObject vfxEffect;
    public float damageRange = 4f;
    public float damagePerSecond = 15f;

    private NavMeshAgent agent;

    [SerializeField] private Transform playerTarget;
    [SerializeField] private PlayerHealth playerHealth;

    private bool playerDetected = false;
    private bool vfxTriggered = false;

    private Coroutine followRoutine;
    private Coroutine damageRoutine;

    [Header("Audio")]
    public AudioSource walkAudioSource;
    public AudioClip walkClip;
    public AudioSource fireSound;
    public AudioClip fireClip;

    [Header("Animation")]
    public Animator animator;

    [Header("Materials")]
    public Renderer enemyRenderer;
    public Material normalMaterial;
    public Material fireMaterial;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyRenderer.material = normalMaterial;

        if (!isServer) return;

        if (vfxEffect != null)
            vfxEffect.SetActive(false);

        SetNewRandomDestination();
    }

    void Update()
    {
        if (!isServer) return;

        HandlePlayerDetection();

        if (animator != null)
            animator.SetFloat("Speed", agent.velocity.magnitude);

        if (!playerDetected)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                SetNewRandomDestination();

            FaceMovementDirection();
        }
    }

    void HandlePlayerDetection()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");

        float closestDist = Mathf.Infinity;
        Transform closestPlayer = null;
        PlayerHealth closestHealth = null;

        foreach (GameObject p in players)
        {
            float d = Vector3.Distance(transform.position, p.transform.position);
            if (d <= detectionRange && d < closestDist)
            {
                closestDist = d;
                closestPlayer = p.transform;
                closestHealth = p.GetComponent<PlayerHealth>();
            }
        }

        if (closestPlayer != null && !playerDetected)
        {
            playerDetected = true;
            playerTarget = closestPlayer;
            playerHealth = closestHealth;

            followRoutine = StartCoroutine(FollowPlayer());
        }
        else if (closestPlayer == null && playerDetected)
        {
            ResetState();
        }
    }

    IEnumerator FollowPlayer()
    {
        while (playerDetected && playerTarget != null)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);

            if (distance > nearRange)
            {
                agent.isStopped = false;
                agent.speed = approachSpeed;

                Vector3 dir = (playerTarget.position - transform.position).normalized;
                agent.SetDestination(playerTarget.position - dir * nearRange);
            }
            else
            {
                agent.isStopped = true;
                FacePlayer();

                if (!vfxTriggered)
                {
                    vfxTriggered = true;
                    isBurningPlayer = true;

                    RpcStartBurnVFX();

                    if (damageRoutine == null)
                        damageRoutine = StartCoroutine(ApplyDamage());
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        ResetState();
    }

    [ServerCallback]
    IEnumerator ApplyDamage()
    {
        while (isBurningPlayer && playerTarget != null)
        {
            float d = Vector3.Distance(transform.position, playerTarget.position);

            if (d <= damageRange && playerHealth != null)
            {
                playerHealth.TakeDamage(damagePerSecond);
            }

            yield return new WaitForSeconds(1f);
        }

        damageRoutine = null;
        vfxTriggered = false;
    }

    [ClientRpc]
    void RpcStartBurnVFX()
    {
        if (vfxEffect != null)
            vfxEffect.SetActive(true);

        if (fireSound != null && fireClip != null)
        {
            fireSound.clip = fireClip;
            fireSound.loop = true;
            fireSound.Play();
        }

        StartCoroutine(SmoothMaterialTransition());
    }

    IEnumerator SmoothMaterialTransition()
    {
        float t = 0f;
        float duration = 1f;

        Material temp = new Material(normalMaterial);

        while (t < duration)
        {
            t += Time.deltaTime;
            temp.Lerp(normalMaterial, fireMaterial, t / duration);
            enemyRenderer.material = temp;
            yield return null;
        }

        enemyRenderer.material = fireMaterial;
    }

    void ResetState()
    {
        isBurningPlayer = false;
        playerDetected = false;
        vfxTriggered = false;

        if (followRoutine != null) StopCoroutine(followRoutine);
        if (damageRoutine != null) StopCoroutine(damageRoutine);

        if (vfxEffect != null)
            vfxEffect.SetActive(false);

        if (fireSound != null && fireSound.isPlaying)
            fireSound.Stop();

        enemyRenderer.material = normalMaterial;

        playerTarget = null;
        playerHealth = null;

        agent.isStopped = false;
        SetNewRandomDestination();
    }

    void SetNewRandomDestination()
    {
        Vector3 rand = Random.insideUnitSphere * moveRadius + transform.position;
        if (NavMesh.SamplePosition(rand, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
        {
            agent.speed = wanderSpeed;
            agent.SetDestination(hit.position);
        }
    }

    void FacePlayer()
    {
        if (playerTarget == null) return;
        Vector3 dir = playerTarget.position - transform.position;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 4f);
    }

    void FaceMovementDirection()
    {
        if (agent.velocity.sqrMagnitude < 0.01f) return;
        Vector3 dir = agent.velocity.normalized;
        dir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
    }
}
