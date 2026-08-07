using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class tutorialtolevel1 : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		if (other.CompareTag("Player"))
		{
			SceneManager.LoadScene("Yash_Shader");
		}
	}
}
