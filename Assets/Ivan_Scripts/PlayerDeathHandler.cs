using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDeathHandler : MonoBehaviour
{
    [Header("Death Settings")]
    [SerializeField] private string respawnSceneName = "tutorial level";
    [SerializeField] private float fadeDuration = 0.8f;

    private Health health;
    private bool isDead;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
        health.OnDeath += HandlePlayerDeath;
    }

    void OnDisable()
    {
        health.OnDeath -= HandlePlayerDeath;
    }

    private void HandlePlayerDeath()
    {
        if (isDead) return;
        isDead = true;

        // Disable renderers and colliders
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.enabled = false;

        var colliders = GetComponentsInChildren<Collider>();
        foreach (var c in colliders) c.enabled = false;

        if (TryGetComponent<PlayerBrain>(out var brain))
        {
            brain.enabled = false;
        }

        // Trigger smooth fade to black and load respawn scene
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeToScene(respawnSceneName, fadeDuration);
        }
    }
}