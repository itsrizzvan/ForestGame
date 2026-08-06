using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem; // Required for the new input system

// Custom container so every attack gets its own animation name and specific duration
[System.Serializable]
public struct AttackData
{
	public string animationName;
	public float duration; // Custom timing per punch to eliminate freezing/stuck frames
}

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
	public enum PlayerState
	{
		Idle,
		Moving,
		Sprinting,
		Dashing,
		Attacking,
		Blocking,
		Grappling
	}

	[Header("Current State")]
	public PlayerState currentState = PlayerState.Idle;

	[Header("Animation")]
	public Animator animator; // Drag your character model here in the inspector
	private int animSpeedParam = Animator.StringToHash("Speed"); // Caching for performance

	[Header("Combo Settings")]
	public AttackData[] comboAttacks; // Configurable per-attack data array
	public float comboCooldown = 0.5f; // Wait time after completing a full combo sequence
	public int maxComboHits = 4; // Max chainable punches before forced reset
	private float lastComboEndTime = 0f;
	private bool nextAttackQueued = false;
	private int lastPunchIndex = -1; // Prevents repeating the exact same animation twice

	[Header("Input Actions")]
	public InputAction moveAction;
	public InputAction aimAction;
	public InputAction attackAction;
	public InputAction blockAction;
	public InputAction sprintAction; // NEW: Dedicated sprint input
	public InputAction dashAction;   // This will now be L3

	[Header("Sprint & Dash Settings")]
	public float sprintSpeed = 15f;
	public float dashSpeed = 25f;
	public float dashDuration = 0.3f;

	[Header("Movement Settings")]
	public float moveSpeed = 8f;
	public float turnSpeed = 15f;
	public float gravity = -9.81f;

	[Header("Combat Settings")]
	public float walkPunchStep = 1.5f;   // Step distance when walking and punching
	public float runPunchStep = 4.0f;    // Step distance when sprinting and punching
	public float attackTurnSpeed = 12f;  // How fast the player can rotate mid-punch

	[Header("Hitbox Settings")]
	public float attackRange = 1f;       // Distance in front of player where hitbox spawns
	public float attackRadius = 1.2f;    // Radius of the punch detection sphere
	public int attackDamage = 10;        // Damage per hit
	public LayerMask enemyLayer;         // Assign to your "Enemy" layer in the inspector

	private CharacterController controller;
	private Camera mainCamera;
	private Vector3 velocity;

	private void OnEnable()
	{
		moveAction.Enable();
		aimAction.Enable();
		attackAction.Enable();
		blockAction.Enable();
		sprintAction.Enable(); // Turn on the new sprint input
		dashAction.Enable();
	}

	private void OnDisable()
	{
		moveAction.Disable();
		aimAction.Disable();
		attackAction.Disable();
		blockAction.Disable();
		sprintAction.Disable(); // Turn it off here too
		dashAction.Disable();
	}

	void Start()
	{
		controller = GetComponent<CharacterController>();
		mainCamera = Camera.main;
	}
	private float inputBufferTimer = 0f;
	void Update()
	{
		ApplyGravity();

		// Count down the buffer timer
		if (inputBufferTimer > 0) inputBufferTimer -= Time.deltaTime;

		// If they pressed attack, fill the buffer with 0.2 seconds of "memory"
		if (attackAction.WasPressedThisFrame())
		{
			inputBufferTimer = 0.2f;
		}

		switch (currentState)
		{
			case PlayerState.Idle:
			case PlayerState.Moving:
			case PlayerState.Sprinting:
				HandleMovement();

				// Check the buffer timer instead of the exact frame
				if (inputBufferTimer > 0 && Time.time >= lastComboEndTime + comboCooldown)
				{
					inputBufferTimer = 0f; // Consume the buffer
					StartCoroutine(PunchComboRoutine());
				}
				else if (blockAction.IsPressed())
				{
					currentState = PlayerState.Blocking;
					animator.CrossFadeInFixedTime("Block", 0.15f);
				}
				break;

			case PlayerState.Attacking:
				// Check the buffer here too!
				if (inputBufferTimer > 0)
				{
					nextAttackQueued = true;
					inputBufferTimer = 0f; // Consume the buffer
				}
				controller.Move(velocity * Time.deltaTime);
				break;

			case PlayerState.Blocking:
				HandleBlocking();
				break;

			case PlayerState.Dashing:
				controller.Move(velocity * Time.deltaTime);
				break;

			case PlayerState.Grappling:
				break;
		}
	}

	private void HandleMovement()
	{
		Vector2 moveInput = moveAction.ReadValue<Vector2>();
		Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

		// Check if the player is holding the Sprint button ('B' / Button East)
		bool isHoldingSprint = sprintAction.IsPressed();

		// 1. CHECK FOR DASH (L3 Press)
		// Only allow a dash if they pressed L3, are holding Sprint, and are actually moving
		if (dashAction.WasPressedThisFrame() && isHoldingSprint && inputDir.magnitude >= 0.1f)
		{
			StartCoroutine(DashRoutine());
			return; // Stop processing normal movement this frame
		}

		// 2. NORMAL MOVEMENT & SPRINTING
		if (inputDir.magnitude >= 0.1f)
		{
			currentState = isHoldingSprint ? PlayerState.Sprinting : PlayerState.Moving;
			float currentSpeed = isHoldingSprint ? sprintSpeed : moveSpeed;

			// Target Animator Speed: 1 for Run, 2 for Sprint
			float targetAnimSpeed = isHoldingSprint ? 2f : 1f;
			animator.SetFloat(animSpeedParam, targetAnimSpeed, 0.1f, Time.deltaTime);

			float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

			Quaternion targetRotation = Quaternion.LookRotation(moveDir);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
			controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime);
		}
		else
		{
			currentState = PlayerState.Idle;
			animator.SetFloat(animSpeedParam, 0f, 0.1f, Time.deltaTime);
		}

		controller.Move(velocity * Time.deltaTime);
	}

	private IEnumerator DashRoutine()
	{
		currentState = PlayerState.Dashing;

		// FAST BLEND IN: Lowered to 0.05f so the dash triggers instantly 
		// and doesn't blend weirdly with the sprint pose.
		animator.CrossFadeInFixedTime("Dash", 0.05f);

		float timer = 0f;
		Vector3 dashDir = transform.forward;

		while (timer < dashDuration)
		{
			controller.Move(dashDir * dashSpeed * Time.deltaTime);
			timer += Time.deltaTime;
			yield return null;
		}

		// Return state to Idle. Because they are holding the sprint button, 
		// the Update loop will instantly change this to Sprinting on the very next frame!
		currentState = PlayerState.Idle;

		// SMOOTH MELT OUT: Increased to 0.35f. This gives the character more time 
		// to naturally unfold from the dash pose back into the upright sprinting pose.
		animator.CrossFadeInFixedTime("Locomotion", 0.35f);
	}

	private void HandleBlocking()
	{
		if (!blockAction.IsPressed())
		{
			currentState = PlayerState.Idle;
			animator.CrossFadeInFixedTime("Locomotion", 0.1f);
		}

		controller.Move(velocity * Time.deltaTime);
	}

	private IEnumerator PunchComboRoutine()
	{
		currentState = PlayerState.Attacking;
		nextAttackQueued = false;
		int currentHitCount = 0;

		Vector3 momentum = controller.velocity;
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

			animator.CrossFadeInFixedTime(currentAttack.animationName, 0.15f);

			// DYNAMIC STEP LOGIC: Check input for THIS specific punch
			float currentPunchStep = 0f;
			Vector2 moveInput = moveAction.ReadValue<Vector2>();

			if (moveInput.magnitude >= 0.1f)
			{
				// If holding the sprint button, use the big step. Otherwise, use the walk step.
				currentPunchStep = dashAction.IsPressed() ? runPunchStep : walkPunchStep;
			}
			// (If magnitude is < 0.1f, currentPunchStep remains 0, meaning NO step while Idle)

			float windUpTime = currentAttack.duration * 0.3f;
			float followThroughTime = currentAttack.duration * 0.4f;
			float cancelWindow = currentAttack.duration * 0.3f;

			nextAttackQueued = false;

			// PHASE 1: WIND-UP
			float timer = 0;
			while (timer < windUpTime)
			{
				SmoothFaceAttackDirection(); // ALLOWS MID-PUNCH ROTATION

				Vector3 lunge = (transform.forward * currentPunchStep) / currentAttack.duration;
				controller.Move((lunge + (momentum * 0.4f)) * Time.deltaTime);
				momentum = Vector3.Lerp(momentum, Vector3.zero, Time.deltaTime * 5f);

				timer += Time.deltaTime;
				yield return null;
			}

			// PHASE 2: THE HIT
			Vector3 hitCenter = transform.position + transform.forward * attackRange;
			Collider[] hitEnemies = Physics.OverlapSphere(hitCenter, attackRadius, enemyLayer);
			foreach (Collider enemyCollider in hitEnemies)
			{
				IDamageable damageable = enemyCollider.GetComponent<IDamageable>();
				if (damageable != null) damageable.TakeDamage(attackDamage);
			}

			// PHASE 3: FOLLOW-THROUGH
			timer = 0;
			while (timer < followThroughTime)
			{
				SmoothFaceAttackDirection(); // ALLOWS MID-PUNCH ROTATION

				Vector3 lunge = (transform.forward * currentPunchStep) / currentAttack.duration;
				controller.Move(lunge * Time.deltaTime);

				timer += Time.deltaTime;
				yield return null;
			}

			// PHASE 4: CANCEL WINDOW
			timer = 0;
			while (timer < cancelWindow)
			{
				SmoothFaceAttackDirection(); // ALLOWS MID-PUNCH ROTATION

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
		currentState = PlayerState.Idle;
		animator.CrossFadeInFixedTime("Locomotion", 0.3f);
	}

	private void SmoothFaceAttackDirection()
	{
		Vector2 aimInput = aimAction.ReadValue<Vector2>();
		Vector2 moveInput = moveAction.ReadValue<Vector2>();

		Vector3 inputDir = Vector3.zero;

		// Prioritize right stick aiming
		if (aimInput.sqrMagnitude > 0.1f)
		{
			inputDir = new Vector3(aimInput.x, 0f, aimInput.y);
		}
		// Fallback to left stick movement
		else if (moveInput.sqrMagnitude > 0.1f)
		{
			inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
		}

		// If the player is pushing a stick, smoothly rotate them relative to the camera
		if (inputDir != Vector3.zero)
		{
			float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
			Vector3 targetDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

			Quaternion targetRotation = Quaternion.LookRotation(targetDir);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, attackTurnSpeed * Time.deltaTime);
		}
	}

	private void ApplyGravity()
	{
		if (controller.isGrounded && velocity.y < 0)
		{
			velocity.y = -2f;
		}
		velocity.y += gravity * Time.deltaTime;
	}

	private void OnDrawGizmosSelected()
	{
		if (attackRadius == 0) return;

		Gizmos.color = Color.red;
		Vector3 hitCenter = transform.position + transform.forward * attackRange;
		Gizmos.DrawWireSphere(hitCenter, attackRadius);
	}

	// This special Unity function intercepts the animation's physical movement.
	// Because we leave it completely empty, it deletes the animation's forward momentum,
	// forcing the 3D model to stay perfectly locked inside our green capsule!
	
}