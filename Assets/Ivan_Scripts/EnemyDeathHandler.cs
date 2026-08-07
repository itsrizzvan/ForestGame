using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeathHandler : MonoBehaviour
{
    private Health health;
    private WaveManager waveManager;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.OnDeath += HandleDeath;
    }

    void OnDisable()
    {
        health.OnDeath -= HandleDeath;
    }

    public void Setup(WaveManager manager)
    {
        waveManager = manager;
    }

    private void HandleDeath()
    {
        if (waveManager)
        {
            waveManager.OnEnemyDied(gameObject);
        }

        Destroy(gameObject);
    }
}