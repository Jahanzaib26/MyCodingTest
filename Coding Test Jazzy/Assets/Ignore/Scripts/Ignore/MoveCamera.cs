using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamera : MonoBehaviour
{
    public Transform cameraPosition; // MUST be the Camera itself

    void LateUpdate()
    {
        if (cameraPosition == null) return;

        transform.position = cameraPosition.position;
    }

    public void SetTarget(Transform newTarget)
    {
        cameraPosition = newTarget;
    }
}
