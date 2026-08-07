using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damageAmount = 10;
    public LayerMask targetLayers;

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & targetLayers) != 0)
        {
            if (other.TryGetComponent<IDamageable>(out var damageable))
            {
                damageable.TakeDamage(damageAmount);
            }
        }
    }
}