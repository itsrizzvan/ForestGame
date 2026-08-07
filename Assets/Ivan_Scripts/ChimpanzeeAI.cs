using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator), typeof(Health))]
public class ChimpanzeeAI : MonoBehaviour
{
    [Header("Settings & Targets")]
    public float detectionRadius = 8f;
    public float attackRange = 2.0f;
    public float watchDistance = 5.5f;
    public float rotationSpeed = 8f;
    public Transform playerTransform;

    [Header("Combat & Damage")]
    public int attackDamage = 10;
    public float hitBoxRadius = 1.2f;
    public LayerMask playerLayer;

    [Header("Hit Reaction & Step Back")]
    [Tooltip("How far backward the chimp steps when struck by the player")]
    public float stepBackDistance = 1.2f;
    [Tooltip("How long the step-back movement takes")]
    public float stepBackDuration = 0.25f;
    [Tooltip("How long the chimp stays stunned/recovering after stepping back")]
    public float stunRecoveryTime = 0.2f;

    private NavMeshAgent agent;
    private Animator anim;
    private WaveManager waveManager;

    private bool isDetected;
    private bool isAttacking;
    private bool isStunned;
    private bool hasPermission;
    private float cooldownTimer;
    private float offsetAngle;
    private float stateTime;

    private enum State { Waiting, Retreating, Attacking, Stunned }
    private State currentState = State.Waiting;
    private Vector3 targetWatchPos;
    private Coroutine attackCoroutine;

    public void Initialize(WaveManager manager, int spawnIdx)
    {
        waveManager = manager;
        offsetAngle = spawnIdx * (360f / Mathf.Max(1, manager.totalChimpanzeesToSpawn)) + Random.Range(-15f, 15f);

        if (spawnIdx >= 2)
        {
            cooldownTimer = Random.Range(2.0f, 4.0f);
        }
    }

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        if (!playerTransform)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        agent.speed = Random.Range(3.8f, 4.4f);
        agent.stoppingDistance = 0.5f;
        anim.speed = Random.Range(0.95f, 1.1f);

        if (cooldownTimer > 0)
        {
            StartRetreating();
        }
    }

    void Update()
    {
        if (!playerTransform || isStunned) return;

        stateTime += Time.deltaTime;
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

    // Called automatically by Health.cs when taking damage
    public void ApplyHitStun()
    {
        if (gameObject.activeInHierarchy)
        {
            StartCoroutine(StepBackStunRoutine());
        }
    }

    private IEnumerator StepBackStunRoutine()
    {
        isStunned = true;
        currentState = State.Stunned;

        // 1. Interrupt active attack or movement instantly
        if (attackCoroutine != null)
        {
            StopCoroutine(attackCoroutine);
            isAttacking = false;
        }

        agent.isStopped = true;
        agent.updateRotation = false;

        // 2. Trigger Hit Animation & Reset Movement Parameters
        anim.SetTrigger("Hit");
        anim.SetBool("IsRunning", false);

        // 3. Calculate Step-Back Direction (Directly away from player)
        Vector3 retreatDir = (transform.position - playerTransform.position).normalized;
        retreatDir.y = 0; // Lock to ground plane

        if (retreatDir == Vector3.zero)
            retreatDir = -transform.forward;

        // 4. Perform Code-Driven Step Back while Animation Plays
        float timer = 0f;
        float speed = stepBackDistance / stepBackDuration;

        while (timer < stepBackDuration)
        {
            agent.Move(retreatDir * speed * Time.deltaTime);
            RotateToPlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        // 5. Stun Recovery Pause
        if (stunRecoveryTime > 0)
        {
            yield return new WaitForSeconds(stunRecoveryTime);
        }

        isStunned = false;

        // 6. IMMEDIATE COUNTER ATTACK
        cooldownTimer = 0f;
        if (!hasPermission && waveManager)
        {
            hasPermission = waveManager.RequestAttackPermission();
        }

        if (hasPermission)
        {
            attackCoroutine = StartCoroutine(AttackSequenceRoutine());
        }
        else
        {
            StartRetreating();
        }
    }

    void HandleCombatState(float distToPlayer)
    {
        if (!hasPermission && cooldownTimer <= 0 && waveManager)
        {
            hasPermission = waveManager.RequestAttackPermission();
        }

        if (hasPermission)
        {
            attackCoroutine = StartCoroutine(AttackSequenceRoutine());
            return;
        }

        if (currentState == State.Retreating)
        {
            bool reached = Vector3.Distance(transform.position, targetWatchPos) <= 1.5f;
            bool stuck = stateTime > 1.2f && agent.hasPath && agent.velocity.sqrMagnitude < 0.05f;

            if (reached || stuck)
            {
                EnterWaitingState();
            }
            else
            {
                bool isPhysicallyMoving = agent.velocity.sqrMagnitude > 0.1f;
                anim.SetBool("IsRunning", isPhysicallyMoving);
            }
        }
        else
        {
            agent.isStopped = true;
            agent.updateRotation = false;
            RotateToPlayer();

            if (stateTime > 1.0f && (distToPlayer > watchDistance + 3.0f || distToPlayer < watchDistance - 2.0f))
            {
                StartRetreating();
            }
        }
    }

    void EnterWaitingState()
    {
        currentState = State.Waiting;
        stateTime = 0f;
        agent.isStopped = true;
        agent.updateRotation = false;

        anim.SetBool("IsRunning", false);
        anim.SetBool("IsDetected", false);
        anim.SetInteger("IdleIndex", Random.Range(1, 3));

        RotateToPlayer();
    }

    void StartRetreating()
    {
        currentState = State.Retreating;
        stateTime = 0f;
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

        int comboCount = Random.Range(1, 4);

        for (int i = 0; i < comboCount; i++)
        {
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

            agent.isStopped = true;
            agent.updateRotation = false;
            anim.SetBool("IsRunning", false);
            anim.SetTrigger("Attack" + Random.Range(1, 3));

            bool damageDealt = false;

            for (float t = 0; t < 1.1f; t += Time.deltaTime)
            {
                if (t < 0.35f) RotateToPlayer();

                if (!damageDealt && t >= 0.3f && t <= 0.5f)
                {
                    PerformHitCheck();
                    damageDealt = true;
                }

                yield return null;
            }

            if (i < comboCount - 1)
            {
                yield return new WaitForSeconds(Random.Range(0.2f, 0.4f));
            }
        }

        if (hasPermission && waveManager)
        {
            waveManager.ReleaseAttackPermission();
            hasPermission = false;
        }

        offsetAngle += Random.Range(40f, 120f);
        cooldownTimer = Random.Range(3.0f, 6.0f);
        isAttacking = false;

        StartRetreating();
    }

    void PerformHitCheck()
    {
        Vector3 hitCenter = transform.position + transform.forward * (attackRange * 0.8f);
        Collider[] hitTargets = Physics.OverlapSphere(hitCenter, hitBoxRadius, playerLayer);

        foreach (Collider target in hitTargets)
        {
            if (target.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(attackDamage);
            }
        }
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 hitCenter = transform.position + transform.forward * (attackRange * 0.8f);
        Gizmos.DrawWireSphere(hitCenter, hitBoxRadius);
    }
}