using UnityEngine;

public class SteamBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (!SteamManager.Initialized)
        {
            gameObject.AddComponent<SteamManager>();
        }
    }
}
