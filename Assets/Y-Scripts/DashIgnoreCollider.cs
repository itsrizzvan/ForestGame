using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DashIgnoreCollider : MonoBehaviour
{
	[Tooltip("The player to monitor. If left empty, it will auto-find the PlayerBrain.")]
	public PlayerBrain playerBrain;

	private Collider targetCollider;

	void Awake()
	{
		targetCollider = GetComponent<Collider>();

		// Auto-assign the player if you forget to drag it in the Inspector
		if (playerBrain == null)
		{
			// NEW: Updated to use Unity's modern, optimized lookup method
			playerBrain = FindFirstObjectByType<PlayerBrain>();
		}
	}

	void Update()
	{
		// Safety check in case the player hasn't loaded yet
		if (playerBrain == null) return;

		// Check if the player is currently in the Dashing state
		bool isPlayerDashing = (playerBrain.currentState == PlayerBrain.PlayerState.Dashing);

		// If the player is dashing, the collider should be OFF (!isPlayerDashing).
		// We only change it if the current state doesn't match what it should be to save performance.
		if (targetCollider.enabled == isPlayerDashing)
		{
			targetCollider.enabled = !isPlayerDashing;
		}
	}
}