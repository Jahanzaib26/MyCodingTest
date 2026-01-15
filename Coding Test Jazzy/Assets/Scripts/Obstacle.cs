using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Damage Settings")]
    public int reduceAmount = 1; // how many numbers it removes

    [Header("Optional Effects")]
    public ParticleSystem hitEffect;

    private void OnTriggerEnter(Collider other)
    {
        PlayerValueController player = other.GetComponent<PlayerValueController>();
        if (player != null)
        {
            // Reduce player value
            player.ReduceValue(reduceAmount);

            // Play particle effect if assigned
            if (hitEffect != null)
            {
                ParticleSystem effect = Instantiate(hitEffect, transform.position, Quaternion.identity);
                effect.Play();
                Destroy(effect.gameObject, effect.main.duration);
            }

            // Optional: destroy the obstacle after hitting
           // Destroy(gameObject);
        }
    }
}
