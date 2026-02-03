using System.Collections;
using UnityEngine;
using UnityEngine.AI;

using Mirror;

public class SpiderController : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveRadius = 10f;
    public float waitTime = 2f;

    [Header("Detection Settings")]
    public float detectionRange = 8f;
    public float stopDistance = 2.5f;

    [Header("Audio Clips")]
    public AudioSource audioSource;
    public AudioClip randomMoveClip;
    public AudioClip rushClip;

    private NavMeshAgent agent;
    private Animator anim;
    private Vector3 initialPosition;// for storing spider initial position

    private bool playerInRange = false;
    private bool isObserving = false;
    private bool isRushing = false;
    private bool isRandomMoving = false;

    private float detectionTimer = 0f;

    [SerializeField] private Transform playerTarget;
    [SerializeField] private PlayerHealth playerHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        initialPosition = transform.position;

        if (!isServer) return; // 🚨 server only

        playerTarget = null;
        playerHealth = null;

        SetNewRandomDestination();
        PlayCrawlSound(1f);
    }


    void Update()
    {
        if (!isServer) return; // 🚨 SERVER ONLY AI
        HandlePlayerDetection();

        if (anim != null)
            anim.SetFloat("Speed", agent.velocity.magnitude);

        // Normal wandering (jab player detect na ho)
        if (!isRushing && !isObserving && !playerInRange && !isRandomMoving)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                SetNewRandomDestination();
        }

        // Rotation control
        if (isObserving && playerTarget != null)
            FacePlayer();
        else if (isRandomMoving && agent.velocity.magnitude > 0.1f)
            FaceMovementDirection();
    }

    // ✅ New Player Detection logic (BlindManController style)
    void HandlePlayerDetection()
    {
        if (!isServer) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            float distance = Vector3.Distance(transform.position, playerObj.transform.position);

            // 🟢 Player detect hua — target aur health assign karo
            if (distance <= detectionRange && !playerInRange)
            {
                playerTarget = playerObj.transform;
                playerHealth = playerObj.GetComponent<PlayerHealth>();
                playerInRange = true;

                detectionTimer = 0f;
                StartCoroutine(ObservePlayer());
            }
            // 🔴 Player range se bahar gaya — sab null karo
            else if (distance > detectionRange && playerInRange)
            {
                playerInRange = false;
                isObserving = false;
                isRushing = false;
                isRandomMoving = false;
                playerTarget = null;
                playerHealth = null;

                agent.isStopped = false;
                PlayCrawlSound(1f);
                SetNewRandomDestination();
            }
        }
    }

    IEnumerator ObservePlayer()
    {
        isObserving = true;
        agent.isStopped = true;
        anim.SetFloat("Speed", 0f);
        StopSound();

        Debug.Log("🕷️ Spider observing player for 3 seconds...");
        float observeTime = 3f;

        float t = 0;
        while (t < observeTime)
        {
            if (playerTarget != null)
                FacePlayer();
            t += Time.deltaTime;
            yield return null;
        }

        // ✅ Observation done — start approaching player
        if (playerInRange && playerTarget != null)
        {
            Debug.Log("🚶 Spider starts moving toward player...");
            isObserving = false;
            StartCoroutine(MoveTowardPlayerThenRush());
        }
        else
        {
            isObserving = false;
        }
    }

    IEnumerator MoveTowardPlayerThenRush()
    {
        isRandomMoving = true;
        agent.isStopped = false;
        PlayCrawlSound(1f);

        float approachSpeed = 1.5f;   // Normal walk before rushing
        float rushTriggerDistance = 2.5f;
        // Distance to trigger sudden rush

        while (playerTarget != null && playerInRange)
        {
            float distance = Vector3.Distance(transform.position, playerTarget.position);

            // Move normally toward player
            if (agent.enabled && !agent.isStopped)
            {
                agent.speed = approachSpeed;
                agent.SetDestination(playerTarget.position);
            }

            // 🕹️ Rotate toward movement direction
            FaceMovementDirection();

            // 👇 When spider gets close enough — start rush
            if (distance <= rushTriggerDistance)
            {
                Debug.Log("⚡ Spider now RUSHES at player!");
                isRandomMoving = false;
                StartCoroutine(RushAtPlayer());
                yield break;
            }

            // If player escaped
            if (distance > detectionRange)
            {
                Debug.Log("❌ Player escaped — stop chase.");
                isRandomMoving = false;
                playerInRange = false;
                yield break;
            }

            yield return null;
        }

        isRandomMoving = false;
    }


    //IEnumerator RandomMovementBeforeRush()
    //{
    //    isRandomMoving = true;
    //    agent.isStopped = false;
    //    PlayCrawlSound(1f);
    //    SetNewRandomDestination();

    //    float moveDuration = 10f;
    //    float timer = 0f;

    //    while (timer < moveDuration)
    //    {
    //        timer += Time.deltaTime;

    //        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
    //            SetNewRandomDestination();

    //        FaceMovementDirection();

    //        if (playerTarget == null || Vector3.Distance(transform.position, playerTarget.position) > detectionRange)
    //        {
    //            Debug.Log("❌ Player escaped — spider stops rush plan.");
    //            isRandomMoving = false;
    //            playerInRange = false;
    //            yield break;
    //        }

    //        yield return null;
    //    }

    //    // 10 seconds passed — now rush
    //    if (playerInRange && !isRushing)
    //    {
    //        StartCoroutine(RushAtPlayer());
    //    }

    //    isRandomMoving = false;
    //}

    IEnumerator RushAtPlayer()
    {
        if (!isServer) yield break;

        if (playerTarget == null) yield break;

        Debug.Log("⚡ Spider RUSHING toward player!");
        isRushing = true;

        StopSound();
        PlaySound(rushClip, false);

        // 🔴 Disable NavMesh
        agent.isStopped = true;
        agent.enabled = false;

        anim.SetBool("IsRushing", true);

        float rushSpeed = 25f;
        float stopThreshold = 1.5f;

        while (playerTarget != null)
        {
            float dist = Vector3.Distance(transform.position, playerTarget.position);
            if (dist <= stopThreshold)
                break;

            Vector3 targetPos = playerTarget.position;
            targetPos.y = transform.position.y;

            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPos,
                rushSpeed * Time.deltaTime
            );

            Vector3 dir = (targetPos - transform.position).normalized;
            if (dir != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(dir);

            yield return null;
        }

        Debug.Log("💥 Spider reached player!");

        if (playerHealth != null)
        {
            GameObject playerObj = playerHealth.gameObject;

            // Call die logic
            if (isServer && playerHealth != null)
            {
                playerHealth.Die();
            }

            // Disable player
            //playerObj.SetActive(false);

            // Move player to new position
            //playerObj.transform.position = new Vector3(149f, 87f, -9f);

            // Disable player
            //playerObj.SetActive(false);
        }



        anim.SetBool("IsRushing", false);

        yield return new WaitForSeconds(1.5f);

        // 🟢 Re-enable NavMesh
        agent.enabled = true;
        agent.Warp(transform.position);
        agent.isStopped = false;

        isRushing = false;
    }




    void SetNewRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * moveRadius + transform.position;
        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, moveRadius, NavMesh.AllAreas))
            agent.SetDestination(hit.position);
    }

    void FacePlayer()
    {
        if (playerTarget == null) return;

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
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

    // 🎵 Utility
    void PlaySound(AudioClip clip, bool loop)
    {
        if (audioSource == null || clip == null) return;
        audioSource.loop = loop;
        audioSource.clip = clip;
        audioSource.pitch = 1f;
        audioSource.Play();
    }

    void PlayCrawlSound(float pitch)
    {
        if (audioSource == null || randomMoveClip == null) return;
        audioSource.clip = randomMoveClip;
        audioSource.loop = true;
        audioSource.pitch = pitch;
        if (!audioSource.isPlaying)
            audioSource.Play();
    }

    void StopSound()
    {
        if (audioSource == null) return;
        audioSource.Stop();
    }
}
