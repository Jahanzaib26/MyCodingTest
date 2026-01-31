using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class PlayerObjectControler : NetworkBehaviour
{
    // We have to syn player data 

    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIdNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;
    //public PlayerMovementDualSwinging pmds;

    public GameObject playermodel;

    private CustomNetworkManager manager;


    private CustomNetworkManager Manager
    {
        get
        {
            if (manager != null)
            {
                return manager;
            }

            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }


    private void Start()
    {
        playermodel.SetActive(false);
        DontDestroyOnLoad(this.gameObject);   
    }


    private void PlayerReadyUpdate(bool oldValue , bool newValue)
    {
        if (!isClient) return;
        if (LobbyController.Instance == null) return;

        LobbyController.Instance.UpdatePlayerList();
    }



    void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (!isClient) return;
        if (LobbyController.Instance == null) return;

        LobbyController.Instance.UpdatePlayerList();
    }

    public void ChangeReady()
    {
        if (!isLocalPlayer) return;
        CmdSetPlayerReady();
    }

    [Command]
    void CmdSetPlayerReady()
    {
        Ready = !Ready;
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());

        // Send client's SteamID to server (reliable)
        CmdSetPlayerSteamID((ulong)SteamUser.GetSteamID());

        gameObject.name = "LocalGamePlayer";
        LobbyController.Instance.FindLocalPlayer();
        LobbyController.Instance.UpdateLobbyName();
    }

    [Command]
    private void CmdSetPlayerSteamID(ulong steamID)
    {
        PlayerSteamID = steamID;
        Debug.Log($"[Server] PlayerSteamID set to {steamID} for connection {connectionToClient?.connectionId}");
    }


    public override void OnStartClient()
    {

        print("eneere");

        Manager.GamePlayers.Add(this);
        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.UpdatePlayerList();
    }

    public override void OnStopClient()
    {
        Manager.GamePlayers.Remove(this);
        LobbyController.Instance.UpdatePlayerList();

    }

   [Command]
private void CmdSetPlayerName(string playerName)
{
    // Server pe SyncVar assign karo
    PlayerName = playerName;
    }


    private void Update()
    {
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            if (playermodel.activeSelf == false)
            {
                SetPosition();
                playermodel.SetActive(true);
            }


            //if (isLocalPlayer)
            //{

            //    pmds.MovePlayer();
            //}

        }

        if (SceneManager.GetActiveScene().name == "GameScene2")
        {
            if (playermodel.activeSelf == false)
            {
                SetPosition();
                playermodel.SetActive(true);
            }


            //if (isLocalPlayer)
            //{

            //    pmds.MovePlayer();
            //}

        }

    }

    public void SetPosition()
    {
        // Fixed spawn position
        if (SceneManager.GetActiveScene().name == "GameScene")
        {
            transform.position = new Vector3(236f, 38f, 234f);
        }
        if (SceneManager.GetActiveScene().name == "GameScene2")
        {
            transform.position = new Vector3(188f, 127f, 47f);
        }
    }

    public void PlayerNameUpdate(string OldValue , string NewValue) 
    {
        if (isServer)  // server
        {
            this.PlayerName = NewValue;
        }

        if (isClient)   // client
        {

            LobbyController.Instance.UpdatePlayerList();

        }

    } 

    //start game

    public void CanStartGame(string SceneName)
    {

        if (isLocalPlayer)
        {
            cmdCanStartGame(SceneName);
        }
            

    }

    [Command]
    public void cmdCanStartGame(string SceneName)
    {
        manager.StartGame(SceneName);
    }

}
