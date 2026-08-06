using UnityEngine;

public class IsometricCameraFollow_Y : MonoBehaviour
{
	[Header("Targeting")]
	[Tooltip("The object the camera will follow. If left blank, it will auto-find the Player tag.")]
	public Transform target;

	[Header("Camera Feel")]
	[Tooltip("How long it takes the camera to catch up. Lower = snappier, Higher = floatier")]
	[Range(0.01f, 1f)]
	public float smoothTime = 0.15f;

	private Vector3 offset;
	private Vector3 currentVelocity = Vector3.zero;

	void Start()
	{
		// Auto-find the player if you forgot to drag it into the inspector
		if (target == null)
		{
			GameObject player = GameObject.FindGameObjectWithTag("Player");
			if (player != null)
			{
				target = player.transform;
			}
			else
			{
				Debug.LogError("Camera couldn't find the Player! Make sure your player has the 'Player' tag.");
				return;
			}
		}

		// The script calculates the distance based on where you placed the camera in the Editor.
		// This means you can easily tweak the angle in the Scene view without touching code.
		offset = transform.position - target.position;
	}

	// ALWAYS use LateUpdate for cameras. 
	// This ensures the player finishes moving in Update() BEFORE the camera tries to follow them, preventing jitter.
	void LateUpdate()
	{
		if (target == null) return;

		// Calculate where the camera wants to be
		Vector3 targetPosition = target.position + offset;

		// Smoothly glide to that position
		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothTime);
	}
}