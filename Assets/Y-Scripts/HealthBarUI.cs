using UnityEngine;
using UnityEngine.UI; // Required for UI elements like Slider

public class HealthBarUI : MonoBehaviour
{
	[Header("References")]
	[Tooltip("Drag the Player GameObject here")]
	public Health playerHealth;

	[Tooltip("Drag the Slider component here")]
	public Slider healthSlider;

	private void OnEnable()
	{
		// Subscribe to the event when this UI is turned on
		if (playerHealth != null)
		{
			playerHealth.OnHealthChanged += UpdateHealthBar;

			// Force an initial update so it matches the starting health
			UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.MaxHealth);
		}
	}

	private void OnDisable()
	{
		// Always unsubscribe from events to prevent memory leaks!
		if (playerHealth != null)
		{
			playerHealth.OnHealthChanged -= UpdateHealthBar;
		}
	}

	// This method is automatically called whenever the player takes damage or heals
	private void UpdateHealthBar(int currentHealth, int maxHealth)
	{
		healthSlider.maxValue = maxHealth;
		healthSlider.value = currentHealth;
	}
}