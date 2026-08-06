using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class ChimpanzeeAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public bool canUseDetectionState = true;
    public float detectionRadius = 8.0f;
    public float attackRange = 2.0f;
    public float rotationSpeed = 8.0f; // Smooth turn speed

    [Header("References")]
    public Transform playerTransform;

    private NavMeshAgent agent;
    private Animator animator;
    private WaveManager waveManager;
    
    private bool isPlayerDetected = false;
    private bool isAttacking = false;

    // Diversity parameters
    private int assignedIdleIndex = 1;
    private int assignedDetectIndex = 1;
    private int assignedRunIndex = 1;
    private int lastAttackChoice = 0;

    public void Initialize(WaveManager manager, int spawnIndex)
    {
        waveManager = manager;

        // Force opposite animation sets for small groups (Chimp 0 gets Set 1, Chimp 1 gets Set 2)
        assignedIdleIndex = (spawnIndex % 2 == 0) ? 1 : 2;
        assignedDetectIndex = (spawnIndex % 2 == 0) ? 1 : 2;
        assignedRunIndex = (spawnIndex % 2 == 0) ? 1 : 2;

        // Desynchronize detection logic: One plays detect anim, the next charges immediately
        if (spawnIndex % 2 != 0)
        {
            canUseDetectionState = !canUseDetectionState;
        }
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Add speed variation so two chimps don't move at identical rates
        agent.speed = Random.Range(3.6f, 4.3f);
        animator.speed = Random.Range(0.92f, 1.08f);

        animator.SetInteger("IdleIndex", assignedIdleIndex);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (!isPlayerDetected && distanceToPlayer <= detectionRadius)
        {
            TriggerDetection();
        }

        if (isPlayerDetected && !isAttacking)
        {
            HandleChaseAndAttack(distanceToPlayer);
        }
    }

    void TriggerDetection()
    {
        isPlayerDetected = true;

        if (canUseDetectionState)
        {
            StartCoroutine(PlayDetectionRoutine());
        }
        else
        {
            StartRunning();
        }
    }

    IEnumerator PlayDetectionRoutine()
    {
        agent.isStopped = true;
        agent.updateRotation = false; // Disable NavMesh rotation to handle smooth turning manually

        animator.SetInteger("DetectIndex", assignedDetectIndex);
        animator.SetBool("IsDetected", true);

        float timer = 0f;
        float duration = 1.2f;

        // Smoothly rotate toward player while playing detection clip
        while (timer < duration)
        {
            SmoothRotateTowardsPlayer();
            timer += Time.deltaTime;
            yield return null;
        }

        animator.SetBool("IsDetected", false);
        agent.updateRotation = true;
        StartRunning();
    }

    void StartRunning()
    {
        agent.isStopped = false;
        agent.updateRotation = true;
        animator.SetInteger("RunIndex", assignedRunIndex);
        animator.SetBool("IsRunning", true);
    }

    void HandleChaseAndAttack(float distance)
    {
        agent.SetDestination(playerTransform.position);

        if (distance <= attackRange)
        {
            StartCoroutine(ExecuteAttack());
        }
    }

    IEnumerator ExecuteAttack()
    {
        isAttacking = true;
        agent.isStopped = true;
        agent.updateRotation = false;
        animator.SetBool("IsRunning", false);

        // Alternate attack types per strike so the chimp doesn't repeat the same hit consecutively
        int attackChoice = (lastAttackChoice == 1) ? 2 : 1;
        lastAttackChoice = attackChoice;

        if (attackChoice == 1)
            animator.SetTrigger("Attack1");
        else
            animator.SetTrigger("Attack2");

        float timer = 0f;
        float duration = 1.4f;

        while (timer < duration)
        {
            // Only adjust rotation during the wind-up phase of the attack
            if (timer < 0.4f)
            {
                SmoothRotateTowardsPlayer();
            }
            timer += Time.deltaTime;
            yield return null;
        }

        agent.updateRotation = true;
        isAttacking = false;
        StartRunning();
    }

    void SmoothRotateTowardsPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}