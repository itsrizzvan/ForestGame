using System.Collections;
using UnityEngine;

public class PlayerGrapple : MonoBehaviour
{
	[Header("Grapple Settings")]
	public float grappleRange = 15f;
	public LayerMask grappleLayer;

	[Header("Animation Settings")]
	public string grappleStartAnim = "SwingStart";
	public string grappleSwingAnim = "SwingLand";

	[Tooltip("Lower this to 0.15 or 0.2 for a faster, snappier throw!")]
	public float grappleThrowDelay = 0.15f;

	[Header("Physics Settings")]
	public float pullSpeed = 25f; // Increased default speed for a faster swing
	public float swingOffset = 3f;

	[Tooltip("Higher for forward speed, lower vertical boost to land faster!")]
	public float launchForce = 15f;
	public float verticalBoost = 2f; // Lowered significantly so you don't float!

	[Header("Visuals")]
	[Tooltip("Drag a LineRenderer component here")]
	public LineRenderer lineRenderer;
	[Tooltip("An empty GameObject placed on your character's hand")]
	public Transform handTransform;

	private PlayerBrain brain;
	private PlayerLocomotion locomotion;
	private Transform currentGrapplePoint;
	private Vector3 currentRopeEndPosition;

	void Awake()
	{
		brain = GetComponent<PlayerBrain>();
		locomotion = GetComponent<PlayerLocomotion>();

		if (lineRenderer != null) lineRenderer.positionCount = 0;
	}

	public void AttemptGrapple()
	{
		FindNearestGrapplePoint();

		if (currentGrapplePoint != null)
		{
			StartCoroutine(GrappleRoutine());
		}
	}

	private void FindNearestGrapplePoint()
	{
		Collider[] colliders = Physics.OverlapSphere(transform.position, grappleRange, grappleLayer);
		float closestDistance = Mathf.Infinity;
		currentGrapplePoint = null;

		foreach (Collider col in colliders)
		{
			float distance = Vector3.Distance(transform.position, col.transform.position);
			if (distance < closestDistance)
			{
				closestDistance = distance;
				currentGrapplePoint = col.transform;
			}
		}
	}

	private IEnumerator GrappleRoutine()
	{
		brain.currentState = PlayerBrain.PlayerState.Grappling;

		CharacterController cc = GetComponent<CharacterController>();
		cc.enabled = false;

		Vector3 startPos = transform.position;
		Vector3 grapplePos = currentGrapplePoint.position;

		Vector3 swingDir = (grapplePos - startPos);
		swingDir.y = 0;
		if (swingDir.magnitude < 0.1f) swingDir = transform.forward;
		swingDir.Normalize();

		transform.rotation = Quaternion.LookRotation(swingDir);

		if (lineRenderer != null)
		{
			lineRenderer.positionCount = 2;
			currentRopeEndPosition = handTransform != null ? handTransform.position : transform.position;
		}

		locomotion.animator.CrossFadeInFixedTime(grappleStartAnim, 0.05f);
		yield return new WaitForSeconds(grappleThrowDelay);

		Vector3 p0 = transform.position;

		// TIGHTER EXIT: Pulled closer (3f instead of 5f) and forced lower for a faster arc completion
		Vector3 p2 = grapplePos + (swingDir * 3f);
		p2.y = grapplePos.y - 1f;

		// THE DIP: Still pulls you underneath smoothly
		Vector3 p1 = grapplePos + (Vector3.down * swingOffset * 1.5f) - (swingDir * 1f);

		float totalCurveDistance = Vector3.Distance(p0, p2);
		float duration = totalCurveDistance / pullSpeed;
		duration = Mathf.Clamp(duration, 0.2f, 0.5f);

		float timer = 0f;
		bool hasSwappedAnimation = false;

		// --- NEW: Tracking the exact velocity of the curve ---
		Vector3 lastPos = transform.position;
		Vector3 finalSwingVelocity = Vector3.zero;

		// THE FRAME-PERFECT LOOP
		while (true)
		{
			timer += Time.deltaTime;
			float t = timer / duration;

			// Force t to stop exactly at 1 to prevent the stutter frame
			if (t > 1f) t = 1f;

			if (t >= 0.3f && !hasSwappedAnimation)
			{
				locomotion.animator.CrossFadeInFixedTime(grappleSwingAnim, 0.1f);
				hasSwappedAnimation = true;
			}

			float easeT = t * t;
			Vector3 curvePos = Mathf.Pow(1 - easeT, 2) * p0 + 2 * (1 - easeT) * easeT * p1 + Mathf.Pow(easeT, 2) * p2;

			// Calculate the EXACT speed and direction the player is moving this frame
			finalSwingVelocity = (curvePos - lastPos) / Time.deltaTime;
			lastPos = curvePos;

			transform.position = curvePos;

			// Break out immediately after processing the final frame
			if (t >= 1f) break;

			yield return null;
		}

		cc.enabled = true;

		if (lineRenderer != null) lineRenderer.positionCount = 0;

		// --- THE SEAMLESS LAUNCH ---
		// We multiply the final swing velocity by launchForce to scale the momentum.
		// We set verticalBoost to a NEGATIVE number in the inspector to slam you to the ground faster.
		Vector3 exitVelocity = (finalSwingVelocity.normalized * launchForce);
		exitVelocity.y = Mathf.Min(finalSwingVelocity.y, 0f) + verticalBoost;

		locomotion.velocity = exitVelocity;
		brain.currentState = PlayerBrain.PlayerState.Airborne;
	}

	private void LateUpdate()
	{
		if (brain.currentState == PlayerBrain.PlayerState.Grappling && lineRenderer != null && currentGrapplePoint != null)
		{
			Vector3 ropeStart = handTransform != null ? handTransform.position : transform.position;
			currentRopeEndPosition = Vector3.Lerp(currentRopeEndPosition, currentGrapplePoint.position, Time.deltaTime * 35f); // Faster rope shoot
			lineRenderer.SetPosition(0, ropeStart);
			lineRenderer.SetPosition(1, currentRopeEndPosition);
		}
		else if (lineRenderer != null && lineRenderer.positionCount > 0)
		{
			lineRenderer.positionCount = 0;
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (grappleRange == 0) return;
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(transform.position, grappleRange);
	}
}