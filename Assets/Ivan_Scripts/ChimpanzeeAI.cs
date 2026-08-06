using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class ChimpanzeeAI : MonoBehaviour
{
    [Header("Settings & Targets")]
    public float detectionRadius = 8f;
    public float attackRange = 2.0f;
    public float watchDistance = 5.0f;
    public float rotationSpeed = 8f;
    public Transform playerTransform;

    private NavMeshAgent agent;
    private Animator anim;
    private WaveManager waveManager;

    private bool isDetected;
    private bool isAttacking;
    private bool hasPermission;
    private float cooldownTimer;
    private float offsetAngle;

    private enum State { Waiting, Retreating, Attacking }
    private State currentState = State.Waiting;
    private Vector3 targetWatchPos;

    public void Initialize(WaveManager manager, int spawnIdx)
    {
        waveManager = manager;
        // Distribute watch positions in a ring around the player
        offsetAngle = spawnIdx * (360f / Mathf.Max(1, manager.totalChimpanzeesToSpawn)) + Random.Range(-20f, 20f);
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();

        if (!playerTransform)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent.speed = Random.Range(3.8f, 4.4f);
        agent.stoppingDistance = 0.5f;
        anim.speed = Random.Range(0.95f, 1.1f);
    }

    void Update()
    {
        if (!playerTransform) return;

        float distToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (cooldownTimer > 0) cooldownTimer -= Time.deltaTime;

        if (!isDetected && distToPlayer <= detectionRadius)
        {
            isDetected = true;
        }

        if (isDetected && !isAttacking)
        {
            HandleCombatState(distToPlayer);
        }
    }

    void HandleCombatState(float distToPlayer)
    {
        // 1. Request attack slot if available
        if (!hasPermission && cooldownTimer <= 0 && waveManager)
        {
            hasPermission = waveManager.RequestAttackPermission();
        }

        if (hasPermission)
        {
            StartCoroutine(AttackSequenceRoutine());
            return;
        }

        // 2. NON-ATTACKER / RETREAT & WATCH LOGIC
        if (currentState == State.Retreating)
        {
            float distToTarget = Vector3.Distance(transform.position, targetWatchPos);

            // If reached position OR blocked by another chimp collider (velocity ~ 0)
            if (distToTarget <= 1.8f || (agent.hasPath && agent.velocity.sqrMagnitude < 0.05f))
            {
                EnterWaitingState();
            }
            else
            {
                // ONLY play running animation if ACTUALLY moving in world space
                bool isPhysicallyMoving = agent.velocity.sqrMagnitude > 0.1f;
                anim.SetBool("IsRunning", isPhysicallyMoving);
            }
        }
        else // State.Waiting
        {
            EnterWaitingState();

            // Re-calculate retreat point if player moves far away
            if (distToPlayer > watchDistance + 3.5f || distToPlayer < watchDistance - 2.5f)
            {
                StartRetreating();
            }
        }
    }

    void EnterWaitingState()
    {
        currentState = State.Waiting;
        agent.isStopped = true;
        agent.updateRotation = false;

        // Force reset movement anims
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsDetected", false);

        RotateToPlayer();
    }

    void StartRetreating()
    {
        currentState = State.Retreating;
        agent.isStopped = false;
        agent.updateRotation = true;

        Vector3 dir = new Vector3(Mathf.Cos(offsetAngle), 0, Mathf.Sin(offsetAngle));
        targetWatchPos = playerTransform.position + (dir * watchDistance);

        agent.SetDestination(targetWatchPos);
        anim.SetBool("IsRunning", true);
        anim.SetBool("IsDetected", false);
        anim.SetInteger("RunIndex", Random.Range(1, 3));
    }

    IEnumerator AttackSequenceRoutine()
    {
        isAttacking = true;
        currentState = State.Attacking;

        // TELEGRAPH / DETECT
        agent.isStopped = true;
        agent.updateRotation = false;
        anim.SetBool("IsRunning", false);
        anim.SetBool("IsDetected", true);
        anim.SetInteger("DetectIndex", Random.Range(1, 3));

        for (float t = 0; t < 0.9f; t += Time.deltaTime)
        {
            RotateToPlayer();
            yield return null;
        }

        // COMBO STRIKES (1, 2, or 3 random attacks)
        int comboCount = Random.Range(1, 4);

        for (int i = 0; i < comboCount; i++)
        {
            // CHARGE
            agent.isStopped = false;
            agent.updateRotation = true;
            anim.SetBool("IsDetected", false);
            anim.SetBool("IsRunning", true);
            anim.SetInteger("RunIndex", Random.Range(1, 3));

            while (Vector3.Distance(transform.position, playerTransform.position) > attackRange)
            {
                agent.SetDestination(playerTransform.position);
                yield return null;
            }

            // STRIKE
            agent.isStopped = true;
            agent.updateRotation = false;
            anim.SetBool("IsRunning", false);
            anim.SetTrigger("Attack" + Random.Range(1, 3));

            for (float t = 0; t < 1.1f; t += Time.deltaTime)
            {
                if (t < 0.35f) RotateToPlayer();
                yield return null;
            }

            if (i < comboCount - 1)
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 0.4f));
            }
        }

        // RELEASE & RETREAT
        if (hasPermission && waveManager)
        {
            waveManager.ReleaseAttackPermission();
            hasPermission = false;
        }

        offsetAngle += Random.Range(40f, 120f);
        cooldownTimer = Random.Range(2.5f, 5.0f);
        isAttacking = false;

        StartRetreating();
    }

    void RotateToPlayer()
    {
        Vector3 dir = Vector3.ProjectOnPlane(playerTransform.position - transform.position, Vector3.up).normalized;
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    void OnDestroy()
    {
        if (hasPermission && waveManager) waveManager.ReleaseAttackPermission();
    }
}