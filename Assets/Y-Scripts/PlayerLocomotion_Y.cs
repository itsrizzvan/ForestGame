using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerLocomotion : MonoBehaviour
{
	[Header("Animation")]
	public Animator animator;
	private int animSpeedParam = Animator.StringToHash("Speed");

	[Header("Sprint & Dash Settings")]
	public float sprintSpeed = 15f;
	public float dashSpeed = 25f;
	public float dashDuration = 0.3f;

	[Header("Movement Settings")]
	public float moveSpeed = 8f;
	public float turnSpeed = 15f;
	public float gravity = -9.81f;
	public float jumpHeight = 2.5f;

	public Vector3 velocity;
	public float lastJumpTime = 0f;

	private CharacterController controller;
	private Camera mainCamera;
	private PlayerBrain brain;

	void Awake()
	{
		controller = GetComponent<CharacterController>();
		mainCamera = Camera.main;
		brain = GetComponent<PlayerBrain>();
	}

	public void HandleMovement(Vector2 moveInput, bool isHoldingSprint)
	{
		Vector3 inputDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;
		Vector3 horizontalMove = Vector3.zero;

		if (inputDir.magnitude >= 0.1f)
		{
			if (brain.currentState != PlayerBrain.PlayerState.Airborne)
			{
				brain.currentState = isHoldingSprint ? PlayerBrain.PlayerState.Sprinting : PlayerBrain.PlayerState.Moving;
			}
			float currentSpeed = isHoldingSprint ? sprintSpeed : moveSpeed;
			float targetAnimSpeed = isHoldingSprint ? 2f : 1f;
			animator.SetFloat(animSpeedParam, targetAnimSpeed, 0.1f, Time.deltaTime);

			float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + mainCamera.transform.eulerAngles.y;
			Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

			Quaternion targetRotation = Quaternion.LookRotation(moveDir);
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);

			horizontalMove = moveDir.normalized * currentSpeed;
		}
		else
		{
			if (brain.currentState != PlayerBrain.PlayerState.Airborne)
			{
				brain.currentState = PlayerBrain.PlayerState.Idle;
			}
			animator.SetFloat(animSpeedParam, 0f, 0.1f, Time.deltaTime);
		}

		Vector3 finalMovement = horizontalMove + velocity;
		controller.Move(finalMovement * Time.deltaTime);
	}

	public void ApplyGravity()
    {
        // --- NEW: Horizontal Friction System ---
        // Separate the horizontal momentum from the vertical gravity
        Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);
        
        if (horizontalVelocity.magnitude > 0.1f)
        {
            // Use high friction (15f) when walking to stop quickly, and low friction (3f) in the air to allow for satisfying jumps/swings
            float friction = controller.isGrounded ? 15f : 3f; 
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, friction * Time.deltaTime);
            
            // Apply the decayed velocity back to the main velocity vector
            velocity.x = horizontalVelocity.x;
            velocity.z = horizontalVelocity.z;
        }
        else
        {
            velocity.x = 0;
            velocity.z = 0;
        }
        // ---------------------------------------

        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 

            if (brain.currentState == PlayerBrain.PlayerState.Airborne)
            {
                brain.currentState = PlayerBrain.PlayerState.Idle;
                animator.CrossFadeInFixedTime("Locomotion", 0.2f);
            }
        }
        
        velocity.y += gravity * Time.deltaTime;
    }

	public void Jump()
	{
		brain.currentState = PlayerBrain.PlayerState.Airborne;
		velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
		animator.CrossFadeInFixedTime("Jump", 0.1f);
		lastJumpTime = Time.time;
	}

	public IEnumerator DashRoutine()
	{
		brain.currentState = PlayerBrain.PlayerState.Dashing;
		animator.CrossFadeInFixedTime("Dash", 0.05f);

		float timer = 0f;
		Vector3 dashDir = transform.forward;

		while (timer < dashDuration)
		{
			controller.Move(dashDir * dashSpeed * Time.deltaTime);
			timer += Time.deltaTime;
			yield return null;
		}

		brain.currentState = PlayerBrain.PlayerState.Idle;
		animator.CrossFadeInFixedTime("Locomotion", 0.35f);
	}

	public bool IsGrounded() => controller.isGrounded;

	public void ApplyMomentum(Vector3 momentum)
	{
		controller.Move(momentum * Time.deltaTime);
	}

	public Vector3 GetVelocity() => controller.velocity;
}