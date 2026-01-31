using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField]
    private PlayerObjectControler GamePlayerPrefab;

    public List<PlayerObjectControler> GamePlayers { get; } = new List<PlayerObjectControler>();


    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; // unlocked
    }
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {

       

        PlayerObjectControler GamePlayerInstance = Instantiate(GamePlayerPrefab);

        // ✅ Random position set using PlayerMovementController
        PlayerMovementController moveCtrl = GamePlayerInstance.GetComponent<PlayerMovementController>();
        if (moveCtrl != null)
        {
            moveCtrl.SetPosition();
        }

        GamePlayerInstance.ConnectionID = conn.connectionId;
        GamePlayerInstance.PlayerIdNumber = GamePlayers.Count + 1;

        ulong assignedSteamID = 0;
        if (SteamLobby.instance != null && SteamLobby.instance.CurrentLobbyID != 0)
        {
            CSteamID lobbyId = new CSteamID(SteamLobby.instance.CurrentLobbyID);
            int members = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
            if (members > 0)
            {
                CSteamID steam = SteamMatchmaking.GetLobbyMemberByIndex(lobbyId, members - 1);
                assignedSteamID = (ulong)steam;
            }
        }

        GamePlayerInstance.PlayerSteamID = assignedSteamID;

        NetworkServer.AddPlayerForConnection(conn, GamePlayerInstance.gameObject);
    }

    public void StartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
    }
}
