using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;
using UnityEngine.UI;
using System.Linq;
//using Unity.VisualScripting;

public class LobbyController : MonoBehaviour
{
    public static LobbyController Instance;

    //UI elements
    public Text LobbyNameText;

    //players data 
    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;


    //other data 

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated= false;
    private List<PlayerListItem> PlayerListItems = new List<PlayerListItem>();

    public PlayerObjectControler LocalplayerController; // playerobjectcontroller class object  


    // buttons
    public Button StartGameButton;
    public Text ReadyButtonText;


    //manager
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
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }



    public void ReadyPlayer()
    {
        LocalplayerController.ChangeReady();
    }

    public void UpdateButton()
    {
        if (LocalplayerController.Ready)
        {
            ReadyButtonText.text = "UnReady";

        }
        else
        {
            ReadyButtonText.text = "Ready";
        }
    }

    public void CheckIfAllReady()
    {
        if (Manager == null) return;
        if (Manager.GamePlayers == null) return;
        if (LocalplayerController == null) return;

        bool AllReady = true;

        foreach (PlayerObjectControler player in Manager.GamePlayers)
        {
            if (player == null) continue;

            if (!player.Ready)
            {
                AllReady = false;
                break;
            }
        }

        if (AllReady && LocalplayerController.PlayerIdNumber == 1)
            StartGameButton.interactable = true;
        else
            StartGameButton.interactable = false;
    }




    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "Name");

    }

    public void UpdatePlayerList()
    {
        if (!this || !gameObject.activeInHierarchy)
            return;

        // 🔒 CLEAN DESTROYED UI REFERENCES FIRST
        PlayerListItems.RemoveAll(item => item == null);

        if (!PlayerItemCreated)
        {
            CreateHostPlayerItem();
            return;
        }

        if (PlayerListItems.Count < Manager.GamePlayers.Count)
        {
            CreateClientPlayerItem();
        }

        if (PlayerListItems.Count > Manager.GamePlayers.Count)
        {
            RemovePlayerItem();
        }

        UpdatePlayerItem();
    }


    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalplayerController = LocalPlayerObject.GetComponent<PlayerObjectControler>();
    }


    public void CreateHostPlayerItem()
    {
        if (PlayerItemCreated) return;

        foreach (PlayerObjectControler player in Manager.GamePlayers)
        {
            GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

            NewPlayerItemScript.PlayerName = player.PlayerName;
            NewPlayerItemScript.ConnectionID = player.ConnectionID;
            NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
            NewPlayerItemScript.Ready = player.Ready;
            NewPlayerItemScript.SetPlayerVslues();

            NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            NewPlayerItem.transform.localScale = Vector3.one;


            PlayerListItems.Add(NewPlayerItemScript);

        }

        PlayerItemCreated=true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectControler player in Manager.GamePlayers)
        {
            if(!PlayerListItems.Any(b => b.ConnectionID == player.ConnectionID))
            {
                GameObject NewPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem NewPlayerItemScript = NewPlayerItem.GetComponent<PlayerListItem>();

                NewPlayerItemScript.PlayerName = player.PlayerName;
                NewPlayerItemScript.ConnectionID = player.ConnectionID;
                NewPlayerItemScript.PlayerSteamID = player.PlayerSteamID;
                NewPlayerItemScript.Ready = player.Ready;
                NewPlayerItemScript.SetPlayerVslues();

                NewPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                NewPlayerItem.transform.localScale = Vector3.one;


                PlayerListItems.Add(NewPlayerItemScript);
            }
        }
        
        }

    public void UpdatePlayerItem()
    {


        if (!gameObject || !gameObject.activeInHierarchy)
            return;

        foreach (PlayerObjectControler player in Manager.GamePlayers)
        {
            foreach (PlayerListItem item in PlayerListItems)
            {
                if (item == null) continue;

                if (item.ConnectionID == player.ConnectionID)
                {
                    item.PlayerName = player.PlayerName;
                    item.Ready = player.Ready;
                    item.SetPlayerVslues();

                    if (player == LocalplayerController)
                    {
                        UpdateButton();
                    }
                }
            }

        }

        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        for (int i = PlayerListItems.Count - 1; i >= 0; i--)
        {
            PlayerListItem item = PlayerListItems[i];

            if (item == null)
            {
                PlayerListItems.RemoveAt(i);
                continue;
            }

            if (!Manager.GamePlayers.Any(p => p.ConnectionID == item.ConnectionID))
            {
                PlayerListItems.RemoveAt(i);
                Destroy(item.gameObject);
            }
        }
    }



    // start

    public void StartGame(string SceneName)
    {
        LocalplayerController.CanStartGame(SceneName);
    }

    public void SafeUpdatePlayerList()
    {
        if (!this) return;
        if (!gameObject.activeInHierarchy) return;

        UpdatePlayerList();
    }


}
