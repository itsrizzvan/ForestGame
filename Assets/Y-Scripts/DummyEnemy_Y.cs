// DummyEnemy.cs
using UnityEngine;

public class DummyEnemy_Y : MonoBehaviour, IDamageable
{
	public int health = 30;

	public void TakeDamage(int damageAmount)
	{
		health -= damageAmount;
		Debug.Log($"Enemy took {damageAmount} damage! Health is now {health}");

		// Flash red for visual feedback
		GetComponent<Renderer>().material.color = Color.red;
		Invoke(nameof(ResetColor), 0.15f);

		if (health <= 0)
		{
			Die();
		}
	}

	private void ResetColor()
	{
		GetComponent<Renderer>().material.color = Color.white;
	}

	private void Die()
	{
		Debug.Log("Enemy Defeated!");
		Destroy(gameObject);
	}
}