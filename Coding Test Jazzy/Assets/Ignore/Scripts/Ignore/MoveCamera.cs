using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition;

    public bool copyRotation = false; // 🔑 KEY

    void LateUpdate()
    {
        if (cameraPosition == null) return;

        transform.position = cameraPosition.position;

        if (copyRotation)
            transform.rotation = cameraPosition.rotation;
    }

    public void SetTarget(Transform newTarget, bool spectate = false)
    {
        cameraPosition = newTarget;
        copyRotation = spectate;
    }
}
