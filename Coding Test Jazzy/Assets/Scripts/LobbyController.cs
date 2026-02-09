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
        if (Instance == null) { 
        
            Instance= this;
        }

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None; // unlocked
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
        bool AllReady = true;  // assume sab ready hain

        foreach (PlayerObjectControler player in Manager.GamePlayers)
        {
            if (!player.Ready)   // agar koi bhi NOT ready mila
            {
                AllReady = false;
                break;
            }
        }

        if (AllReady)
        {
            // host ka PlayerIdNumber == 1 check karo
            if (LocalplayerController.PlayerIdNumber == 1)
            {
                StartGameButton.interactable = true;  // ✅ host enable karega
            }
            else
            {
                StartGameButton.interactable = false; // ✅ client ke liye disable
            }
        }
        else
        {
            StartGameButton.interactable = false; // ✅ sab ready nahi -> disable
        }
    }



    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        LobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "Name");

    }

    public void UpdatePlayerList()
    {
        if (!PlayerItemCreated)
        {
            CreateHostPlayerItem();// Host
        }

            if(PlayerListItems.Count < Manager.GamePlayers.Count)
            {
                CreateClientPlayerItem();
            }

            if(PlayerListItems.Count > Manager.GamePlayers.Count)
            {
                RemovePlayerItem();
            }

            if(PlayerListItems.Count == Manager.GamePlayers.Count)
            {
                UpdatePlayerItem();
            }

            
        
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        LocalplayerController = LocalPlayerObject.GetComponent<PlayerObjectControler>();
    }


    public void CreateHostPlayerItem()
    {
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
        foreach (PlayerObjectControler player in Manager.GamePlayers)
        {
            foreach(PlayerListItem PlayerListItemScript in PlayerListItems)
            {
                if (PlayerListItemScript.ConnectionID == player.ConnectionID)
                {
                    PlayerListItemScript.PlayerName = player.PlayerName;
                    PlayerListItemScript.Ready = player.Ready;
                    PlayerListItemScript.SetPlayerVslues();

                    if(player == LocalplayerController)
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
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem playerListItem in  PlayerListItems)
        {
            if(!Manager.GamePlayers.Any(b=> b.ConnectionID == playerListItem.ConnectionID))
            {
                playerListItemToRemove.Add(playerListItem);
            }

        }

        if(playerListItemToRemove.Count > 0)
        {
            foreach( PlayerListItem playerlistItemToRemove in playerListItemToRemove)
            {
                GameObject ObjectToRemove = playerlistItemToRemove.gameObject;
                PlayerListItems.Remove(playerlistItemToRemove);
                Destroy(ObjectToRemove);
                ObjectToRemove= null;

            }
        }
    }


    // start

    public void StartGame(string SceneName)
    {
        LocalplayerController.CanStartGame(SceneName);
    }

    public void BackToMainMenu()
    {
        Debug.Log("⬅️ Back button pressed");

        // 🔹 Steam lobby leave
        if (CurrentLobbyID != 0)
        {
            SteamMatchmaking.LeaveLobby(new CSteamID(CurrentLobbyID));
            Debug.Log("🚪 Left Steam Lobby");
        }

        // 🔹 Agar HOST hai
        if (LocalplayerController != null && LocalplayerController.PlayerIdNumber == 1)
        {
            Debug.Log("🛑 Stopping Host");
            Manager.StopHost();
        }
        else
        {
            Debug.Log("🛑 Stopping Client");
            Manager.StopClient();
        }

        // 🔹 Cursor unlock (menu ke liye)
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }





}
