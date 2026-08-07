using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
	[Header("Input Actions")]
	public InputAction moveAction;
	public InputAction aimAction;
	public InputAction attackAction;
	public InputAction blockAction;
	public InputAction sprintAction;
	public InputAction dashAction;
	public InputAction jumpAction;

	public Vector2 MoveInput => moveAction.ReadValue<Vector2>();
	public Vector2 AimInput => aimAction.ReadValue<Vector2>();
	public bool AttackTriggered => attackAction.WasPressedThisFrame();
	public bool IsBlocking => blockAction.IsPressed();
	public bool IsSprinting => sprintAction.IsPressed();
	public bool DashTriggered => dashAction.WasPressedThisFrame();
	public bool JumpTriggered => jumpAction.WasPressedThisFrame();

	// For the input buffer logic
	public float InputBufferTimer { get; set; } = 0f;

	private void OnEnable()
	{
		moveAction.Enable();
		aimAction.Enable();
		attackAction.Enable();
		blockAction.Enable();
		sprintAction.Enable();
		dashAction.Enable();
		jumpAction.Enable();
	}

	private void OnDisable()
	{
		moveAction.Disable();
		aimAction.Disable();
		attackAction.Disable();
		blockAction.Disable();
		sprintAction.Disable();
		dashAction.Disable();
		jumpAction.Disable();
	}

	private void Update()
	{
		if (InputBufferTimer > 0) InputBufferTimer -= Time.deltaTime;
		if (AttackTriggered) InputBufferTimer = 0.2f;
	}
}