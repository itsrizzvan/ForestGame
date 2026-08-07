using UnityEngine;

[RequireComponent(typeof(PlayerInputHandler))]
[RequireComponent(typeof(PlayerLocomotion))]
[RequireComponent(typeof(PlayerCombat_Y))]
public class PlayerBrain : MonoBehaviour
{
	public enum PlayerState
	{
		Idle,
		Moving,
		Sprinting,
		Dashing,
		Airborne,
		Attacking,
		Blocking,
		Grappling
	}

	[Header("Current State")]
	public PlayerState currentState = PlayerState.Idle;

	private PlayerInputHandler input;
	private PlayerLocomotion locomotion;
	private PlayerCombat_Y combat;

	void Awake()
	{
		input = GetComponent<PlayerInputHandler>();
		locomotion = GetComponent<PlayerLocomotion>();
		combat = GetComponent<PlayerCombat_Y>();
	}

	void Update()
	{
		locomotion.ApplyGravity();

		switch (currentState)
		{
			case PlayerState.Idle:
			case PlayerState.Moving:
			case PlayerState.Sprinting:

				// 1. Dash Check
				if (input.DashTriggered && input.IsSprinting && input.MoveInput.magnitude >= 0.1f && currentState != PlayerState.Airborne)
				{
					StartCoroutine(locomotion.DashRoutine());
					return;
				}

				// 2. Normal Movement
				locomotion.HandleMovement(input.MoveInput, input.IsSprinting);

				// 3. Jump Check
				if (input.JumpTriggered && locomotion.IsGrounded() && Time.time > locomotion.lastJumpTime + 0.3f)
				{
					locomotion.Jump();
				}

				// 4. Combat Checks
				if (input.InputBufferTimer > 0 && Time.time >= combat.lastComboEndTime + combat.comboCooldown)
				{
					input.InputBufferTimer = 0f;
					combat.StartCombo();
				}
				else if (input.IsBlocking)
				{
					currentState = PlayerState.Blocking;
					locomotion.animator.CrossFadeInFixedTime("Block", 0.15f);
				}
				break;

			case PlayerState.Airborne:
				locomotion.HandleMovement(input.MoveInput, input.IsSprinting);
				break;

			case PlayerState.Blocking:
				combat.HandleBlocking();
				break;

			case PlayerState.Attacking:
				if (input.InputBufferTimer > 0)
				{
					combat.QueueNextAttack();
					input.InputBufferTimer = 0f;
				}
				locomotion.ApplyMomentum(locomotion.velocity);
				break;

			case PlayerState.Dashing:
				locomotion.ApplyMomentum(locomotion.velocity);
				break;
		}
	}
}