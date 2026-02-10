using UnityEngine;
using Mirror;

public class FlagTrigger : NetworkBehaviour
{
    // Flag trigger zone
    private void OnTriggerEnter(Collider other)
    {
        // Sirf player objects check karen
        PlayerHealth ph = other.GetComponent<PlayerHealth>();
        if (ph == null) return;

        // Server pe logic chalega, kyunki SyncVar update sab clients pe jayega
        if (isServer)
        {
            CheckLevelStatus();
        }
    }

    [Server] // Sirf server ye check karega
    void CheckLevelStatus()
    {
        int total = TotalCollectManager.Instance.totalCollect;

        if (total <= 0)
        {
            // Level complete
            RpcShowMessageToAll("🎉 Level Complete!");
        }
        else
        {
            // Quota complete karo pehle
            RpcShowMessageToAll($"⚠️ Complete the quota first! Remaining: {total}");
        }
    }

    [ClientRpc] // Sab clients ke liye ye call hoga
    void RpcShowMessageToAll(string message)
    {
        Debug.Log(message);
        // Agar UI text show karna ho to yahan call karo:
        // UIManager.Instance.ShowMessage(message);
    }
}
