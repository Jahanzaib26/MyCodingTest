using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class PlayerTrigger : NetworkBehaviour
{

    //public GameObject StoreText;
    private static bool winnerDeclared = false;

    private void OnTriggerEnter(Collider other)
    {
        // 🔥 SERVER authoritative trigger
        if (!isServer) return;

        if (other.CompareTag("Success"))
        {
            TryDeclareWinner(other.gameObject);
        }

        //if (other.CompareTag("store"))
        //{
        //    StoreText.SetActive(true);
        //}

    }


    private void OnTriggerExit(Collider other)
    {
        //if (other.CompareTag("store"))
        //{
        //    StoreText.SetActive(false);
        //}
    }

        void TryDeclareWinner(GameObject successObject)
    {
        if (winnerDeclared) return;

        winnerDeclared = true;

        uint winnerNetId = netId;

        // 🔥 destroy success object on server
        NetworkServer.Destroy(successObject);

        // Winner ko success
        TargetShowSuccess(connectionToClient);

        // Baqi sab ko fail
        RpcShowFailForOthers(winnerNetId);
    }

    [TargetRpc]
    void TargetShowSuccess(NetworkConnectionToClient target)
    {
        GameManager.Instance.ShowSuccess();
    }

    [ClientRpc]
    void RpcShowFailForOthers(uint winnerNetId)
    {
        if (netId == winnerNetId) return;

        GameManager.Instance.ShowFail();
    }

    // 🔥 Call this when new match starts
    public static void ResetWinner()
    {
        winnerDeclared = false;
    }
}
