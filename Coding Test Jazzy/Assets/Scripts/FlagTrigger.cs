using UnityEngine;
using Mirror;

public class FlagTrigger : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        if (isServer)
        {
            CheckLevelStatus();
        }
    }

    [Server]
    void CheckLevelStatus()
    {
        int total = TotalCollectManager.Instance.totalCollect;

        if (total <= 0)
        {
            // Sab players ko message + win panel
            RpcLevelComplete();
        }
        else
        {
            RpcShowMessageToAll($"⚠️ Complete the quota first! Remaining: {total}");
        }
    }

    [ClientRpc]
    void RpcLevelComplete()
    {
        Debug.Log("🎉 Level Complete!");

        // Har client apna win panel khud show karega
        PlayerMovementDualSwinging localPlayer = FindLocalPlayer();
        if (localPlayer != null)
        {
            localPlayer.showwinpannel();
        }
    }

    [ClientRpc]
    void RpcShowMessageToAll(string message)
    {
        Debug.Log(message);
    }

    PlayerMovementDualSwinging FindLocalPlayer()
    {
        foreach (var player in FindObjectsOfType<PlayerMovementDualSwinging>())
        {
            if (player.isLocalPlayer)
                return player;
        }
        return null;
    }
}
