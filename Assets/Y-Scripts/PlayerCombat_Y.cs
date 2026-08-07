using System.Collections;
using UnityEngine;

[System.Serializable]
public struct AttackData
{
    public string animationName;
    public float duration;
}

public class PlayerCombat_Y : MonoBehaviour
{
    [Header("Combo Settings")]
    public AttackData[] comboAttacks;
    public float comboCooldown = 0.5f;
    public int maxComboHits = 4;
    public float lastComboEndTime = 0f;

    [Header("Combat Settings")]
    public float walkPunchStep = 1.5f;
    public float runPunchStep = 4.0f;
    public float attackTurnSpeed = 12f;

    [Header("Hitbox Settings")]
    public float attackRange = 1f;
    public float attackRadius = 1.2f;
    public int attackDamage = 10;
    public LayerMask enemyLayer;

    private int lastPunchIndex = -1;
    private bool nextAttackQueued = false;

    private PlayerBrain brain;
    private PlayerInputHandler input;
    private PlayerLocomotion locomotion;
    private Camera mainCamera;

    void Awake()
    {
        brain = GetComponent<PlayerBrain>();
        input = GetComponent<PlayerInputHandler>();
        locomotion = GetComponent<PlayerLocomotion>();
        mainCamera = Camera.main;
    }

    public void QueueNextAttack()
    {
        nextAttackQueued = true;
    }

    public void StartCombo()
    {
        StartCoroutine(PunchComboRoutine());
    }

    private IEnumerator PunchComboRoutine()
    {
        brain.currentState = PlayerBrain.PlayerState.Attacking;
        nextAttackQueued = false;
        int currentHitCount = 0;

        Vector3 momentum = locomotion.GetVelocity();
        momentum.y = 0;

        while (currentHitCount < maxComboHits)
        {
            int randomPunchIndex = Random.Range(0, comboAttacks.Length);
            if (comboAttacks.Length > 1)
            {
                while (randomPunchIndex == lastPunchIndex) randomPunchIndex = Random.Range(0, comboAttacks.Length);
            }
            lastPunchIndex = randomPunchIndex;

            AttackData currentAttack = comboAttacks[randomPunchIndex];
            locomotion.animator.CrossFadeInFixedTime(currentAttack.animationName, 0.15f);

            float currentPunchStep = 0f;
            if (input.MoveInput.magnitude >= 0.1f)
            {
                currentPunchStep = input.IsSprinting ? runPunchStep : walkPunchStep;
            }

            float windUpTime = currentAttack.duration * 0.3f;
            float followThroughTime = currentAttack.duration * 0.4f;
            float cancelWindow = currentAttack.duration * 0.3f;

            nextAttackQueued = false;

            // PHASE 1: WIND-UP
            float timer = 0;
            while (timer < windUpTime)
            {
                SmoothFaceAttackDirection();
                Vector3 lunge = (transform.forward * currentPunchStep) / currentAttack.duration;
                locomotion.ApplyMomentum(lunge + (momentum * 0.4f));
                momentum = Vector3.Lerp(momentum, Vector3.zero, Time.deltaTime * 5f);
                timer += Time.deltaTime;
                yield return null;
            }

            // PHASE 2: THE HIT
            Vector3 hitCenter = transform.position + transform.forward * attackRange;
            Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRadius, enemyLayer);
            foreach (Collider enemyCollider in hitEnemies)
            {
                if (enemyCollider.TryGetComponent<IDamageable>(out var damageable))
                {
                    damageable.TakeDamage(attackDamage);
                }
            }

            // PHASE 3: FOLLOW-THROUGH
            timer = 0;
            while (timer < followThroughTime)
            {
                SmoothFaceAttackDirection();
                Vector3 lunge = (transform.forward * currentPunchStep) / currentAttack.duration;
                locomotion.ApplyMomentum(lunge);
                timer += Time.deltaTime;
                yield return null;
            }

            // PHASE 4: CANCEL WINDOW
            timer = 0;
            while (timer < cancelWindow)
            {
                SmoothFaceAttackDirection();
                if (nextAttackQueued) break;
                timer += Time.deltaTime;
                yield return null;
            }

            if (nextAttackQueued)
            {
                currentHitCount++;
                nextAttackQueued = false;
            }
            else
            {
                break;
            }
        }

        lastComboEndTime = Time.time;
        brain.currentState = PlayerBrain.PlayerState.Idle;
        locomotion.animator.CrossFadeInFixedTime("Locomotion", 0.3f);
    }

    private void SmoothFaceAttackDirection()
    {
        Vector3 inputDir = Vector3.zero;

        if (input.AimInput.sqrMagnitude > 0.1f)
        {
            inputDir = new Vector3(input.AimInput.x, 0f, input.AimInput.y);
        }
        else if (input.MoveInput.sqrMagnitude > 0.1f)
        {
            inputDir = new Vector3(input.MoveInput.x, 0f, input.MoveInput.y);
        }

        if (inputDir != Vector3.zero)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
            Vector3 targetDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            Quaternion targetRotation = Quaternion.LookRotation(targetDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, attackTurnSpeed * Time.deltaTime);
        }
    }

    public void HandleBlocking()
    {
        if (!input.IsBlocking)
        {
            brain.currentState = PlayerBrain.PlayerState.Idle;
            locomotion.animator.CrossFadeInFixedTime("Locomotion", 0.1f);
        }
        locomotion.ApplyMomentum(locomotion.velocity);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackRadius == 0) return;
        Gizmos.color = Color.red;
        Vector3 hitCenter = transform.position + transform.forward * attackRange;
        Gizmos.DrawWireSphere(hitCenter, attackRadius);
    }
}