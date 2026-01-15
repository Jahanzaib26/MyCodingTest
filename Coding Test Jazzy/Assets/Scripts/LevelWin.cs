using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelWin : MonoBehaviour
{
    public GameObject winPanel;
    [Header("Optional Effects")]
    public ParticleSystem hitEffect;
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if the collider is the player
        PlayerValueController player = other.GetComponent<PlayerValueController>();
        if (player != null)
        {
            winPanel.SetActive(true);
            // Optional: destroy obstacle after hitting
            Destroy(gameObject);
        }
    }
}
