using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
//using Unity.VisualScripting;

public class SteamLobby : MonoBehaviour
{
    public static SteamLobby instance;


  // callbacks

    protected Callback<LobbyCreated_t> LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> JoinRequest;
    protected Callback<LobbyEnter_t> LobbyEntered;


    //  Lobbies Callbacks
    protected Callback<LobbyMatchList_t> LobbyList;
    protected Callback<LobbyDataUpdate_t> LobbyDataUpdated;


    public List<CSteamID> lobbyIDs = new List<CSteamID>();




    // Variables

    public ulong CurrentLobbyID;

    public const string HostAddressKey = "HostAddress";

    private CustomNetworkManager manager;


    //Gameobjects

    public GameObject HostButton;
    public Text LobbyNameText;



    private void Start()
    {
        if (!SteamManager.Initialized) { return; }

        if (instance == null) { instance = this; }

        manager = GetComponent<CustomNetworkManager>();

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);
        LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);

        LobbyList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdated = Callback<LobbyDataUpdate_t>.Create(OnGetLoobbyData);

    }

    public void HostLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
    }



    //Now make functions for the callbacks


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        Debug.Log("Lobby Created Successfully");
        Debug.Log(SteamUser.GetSteamID().ToString());

        manager.StartHost();




        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());      //in argument we need 3 variables 1 steamid,data we want to edit,what we want to edit 

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),"name", SteamFriends.GetPersonaName().ToString() + "  Lobbby");

    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to join Lobby");

        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback) {

        //// Everyone 

        HostButton.SetActive(false);

        CurrentLobbyID = callback.m_ulSteamIDLobby;
        LobbyNameText.gameObject.SetActive(false);
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name");

        //Clients

        if (NetworkServer.active) { return; }

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);

        manager.StartClient();
    }


    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }


    public void GetLobbiesList()
    {
        if(lobbyIDs.Count > 0)
        {
            lobbyIDs.Clear();
        }

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();
    }


    public void OnGetLobbyList(LobbyMatchList_t result)
    {
        if(LobbiesListManager.instance.ListOfLobbies.Count > 0)
        {
            LobbiesListManager.instance.DestroyLobbies();
        }

        for(int i=0; i<result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }


    public void OnGetLoobbyData(LobbyDataUpdate_t result)
    {
        LobbiesListManager.instance.DisplayLobbies(lobbyIDs, result);
    }

}
