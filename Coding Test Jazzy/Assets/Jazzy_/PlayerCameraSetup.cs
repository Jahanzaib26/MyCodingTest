using Mirror;
using UnityEngine;

public class PlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private GameObject cameraHolder;

    void Start()
    {
        if (!isLocalPlayer)
        {
            cameraHolder.SetActive(false);
        }
        else
        {
            cameraHolder.SetActive(true);
        }
    }
}
