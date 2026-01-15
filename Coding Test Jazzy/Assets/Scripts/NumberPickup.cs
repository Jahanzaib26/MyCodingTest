using UnityEngine;

public class NumberPickup : MonoBehaviour
{
    [Header("Pickup Value")]
    [Tooltip("Set this to match the number mesh (0-9)")]
    public int value = 1;

    [Header("Optional Effects")]
    public ParticleSystem pickupEffect;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the player
        PlayerValueController player = other.GetComponent<PlayerValueController>();
        if (player != null)
        {
            // Add value to the player
            player.AddValue(value);

           // Play particle if assigned
            if (pickupEffect != null)
                {
                    ParticleSystem effect = Instantiate(pickupEffect, transform.position, Quaternion.identity);
                    effect.Play();
                    Destroy(effect.gameObject, effect.main.duration);
                }

            // Destroy the pickup
            Destroy(gameObject);
        }
    }
}
