using UnityEngine;

public class tutorialrespawn : MonoBehaviour
{
	public GameObject player;
	public Transform spawnpoint;

	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			CharacterController cc = player.GetComponent<CharacterController>();
			if (cc != null)
			{
				cc.enabled = false;
			}
			player.transform.position = spawnpoint.position;

			if (cc != null)
			{
				cc.enabled = true;
			}
		}
	}
}