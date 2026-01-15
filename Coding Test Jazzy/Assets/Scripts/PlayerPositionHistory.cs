using System.Collections.Generic;
using UnityEngine;

public class PlayerPositionHistory : MonoBehaviour
{
    [Header("Position History Settings")]
    public List<Vector3> positions = new List<Vector3>();
    public float recordInterval = 0.05f; // seconds between position records
    public int maxPositions = 1000;      // limit history length

    private float timer = 0f;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= recordInterval)
        {
            // Add current position at the start of the list
            positions.Insert(0, transform.position);
            timer = 0f;
        }

        // Keep the list from growing indefinitely
        if (positions.Count > maxPositions)
            positions.RemoveAt(positions.Count - 1);
    }
}
