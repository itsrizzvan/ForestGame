using System;
using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Visual Feedback")]
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashDuration = 0.15f;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    private Color originalColor;
    private Coroutine flashCoroutine;
    private Material targetMaterial;
    private ChimpanzeeAI chimpAI;

    void Awake()
    {
        currentHealth = maxHealth;
        chimpAI = GetComponent<ChimpanzeeAI>();

        if (!targetRenderer)
            targetRenderer = GetComponentInChildren<Renderer>();

        if (targetRenderer)
        {
            targetMaterial = targetRenderer.material; 
            if (targetMaterial.HasProperty("_BaseColor"))
                originalColor = targetMaterial.GetColor("_BaseColor");
            else if (targetMaterial.HasProperty("_Color"))
                originalColor = targetMaterial.color;
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damageAmount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log($"{gameObject.name} took {damageAmount} damage! Current Health: {currentHealth}");

        // Flash Red Feedback
        if (targetMaterial && gameObject.activeInHierarchy)
        {
            if (flashCoroutine != null) StopCoroutine(flashCoroutine);
            flashCoroutine = StartCoroutine(DamageFlashRoutine());
        }

        // Trigger Hit Stun on Chimpanzee AI
        if (chimpAI != null && currentHealth > 0)
        {
            chimpAI.ApplyHitStun();
        }

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }

    private IEnumerator DamageFlashRoutine()
    {
        SetMaterialColor(flashColor);
        yield return new WaitForSeconds(flashDuration);
        SetMaterialColor(originalColor);
    }

    private void SetMaterialColor(Color color)
    {
        if (targetMaterial.HasProperty("_BaseColor"))
            targetMaterial.SetColor("_BaseColor", color);
        else if (targetMaterial.HasProperty("_Color"))
            targetMaterial.color = color;
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }
}