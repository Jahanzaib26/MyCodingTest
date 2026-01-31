using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class CustomNetworkManager : NetworkManager
{




    [SerializeField] private GameObject portalPrefab;
    private GameObject spawnedPortal;




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
        PlayerObjectControler player = Instantiate(GamePlayerPrefab);

        player.ConnectionID = conn.connectionId;
        player.PlayerIdNumber = numPlayers + 1;

        NetworkServer.AddPlayerForConnection(conn, player.gameObject);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if (sceneName != "GameScene")
            return;

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null)
                continue;

            PlayerObjectControler player =
                conn.identity.GetComponent<PlayerObjectControler>();

            if (player == null)
                continue;

            player.SetPosition();
            player.playermodel.SetActive(true);
        }
    }



    public void StartGame(string SceneName)
    {
        ServerChangeScene(SceneName);
    }
}
