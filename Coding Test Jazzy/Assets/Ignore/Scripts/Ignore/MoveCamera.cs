using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;   // current target

    void Update()
    {
        if (cameraPosition == null) return;
        transform.position = cameraPosition.position;
    }

    // 🆕 ADD THIS
    public void SetTarget(Transform newTarget)
    {
        cameraPosition = newTarget;
    }
}
