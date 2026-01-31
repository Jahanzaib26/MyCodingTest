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
    private Rigidbody rb;

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
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }


    private void Start()
    {
        playermodel.SetActive(false);
       // DontDestroyOnLoad(this.gameObject);   
    }


    private void PlayerReadyUpdate(bool oldValue, bool newValue)
    {
        if (!isClient) return;

        if (LobbyController.Instance == null) return;
        if (!LobbyController.Instance.gameObject.activeInHierarchy) return;

        LobbyController.Instance.SafeUpdatePlayerList();
    }

    public override void OnStartServer()
    {
        if (rb != null)
            rb.isKinematic = false;   // ✅ server simulates physics

        base.OnStartServer();

        NetworkTransformHybrid nth =
            GetComponentInChildren<NetworkTransformHybrid>();

        if (nth != null)
        {
            nth.syncDirection = SyncDirection.ServerToClient;
        }
    }

   



    //void OnReadyChanged(bool oldValue, bool newValue)
    //{
    //    if (!isClient) return;
    //    if (LobbyController.Instance == null) return;

    //    LobbyController.Instance.SafeUpdatePlayerList();
    //}

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



        if (!isServer && rb != null)
            rb.isKinematic = true;

        Manager.GamePlayers.Add(this);

        if (LobbyController.Instance == null)
            return;

        LobbyController.Instance.UpdateLobbyName();
        LobbyController.Instance.SafeUpdatePlayerList();
    }


    public override void OnStopClient()
    {
        if (Manager != null)
            Manager.GamePlayers.Remove(this);

        if (LobbyController.Instance != null &&
            LobbyController.Instance.gameObject != null &&
            LobbyController.Instance.gameObject.activeInHierarchy)
        {
            LobbyController.Instance.SafeUpdatePlayerList();
        }
    }



    [Command]
private void CmdSetPlayerName(string playerName)
{
    // Server pe SyncVar assign karo
    PlayerName = playerName;
    }
    private void Update()
    {
        if (!isClient) return;

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "GameScene" || sceneName == "GameScene2")
        {
            if (!playermodel.activeSelf)
            {
                SetPosition();
                SetActiveRecursively(playermodel, true);
            }
        }
    }
    private void SetActiveRecursively(GameObject obj, bool state)
    {
        obj.SetActive(state);

        foreach (Transform child in obj.transform)
        {
            child.gameObject.SetActive(state);
        }
    }


    //private void Update()
    //{
    //    if (SceneManager.GetActiveScene().name == "GameScene")
    //    {
    //        if (playermodel.activeSelf == false)
    //        {
    //            SetPosition();
    //            playermodel.SetActive(true);
    //        }


    //        //if (isLocalPlayer)
    //        //{

    //        //    pmds.MovePlayer();
    //        //}

    //    }

    //    if (SceneManager.GetActiveScene().name == "GameScene2")
    //    {
    //        if (playermodel.activeSelf == false)
    //        {
    //            SetPosition();
    //            playermodel.SetActive(true);
    //        }


    //        //if (isLocalPlayer)
    //        //{

    //        //    pmds.MovePlayer();
    //        //}

    //    }

    //}

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

    public void PlayerNameUpdate(string oldValue, string newValue)
    {
        if (!isClient) return;
        if (LobbyController.Instance == null) return;

        LobbyController.Instance.SafeUpdatePlayerList();
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
