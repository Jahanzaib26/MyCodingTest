using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;

public class LobbiesListManager : MonoBehaviour
{

    public static LobbiesListManager instance;

    public GameObject lobbiesMenu;
    public GameObject lobbyDataItemPrefab;
    public GameObject lobbyListContent;

    public GameObject LobbiesButton, HostButton;


    public List<GameObject> ListOfLobbies = new List<GameObject>();


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


    public void GetListOfLobbies()
    {
        LobbiesButton.SetActive(false);
        HostButton.SetActive(false);


        lobbiesMenu.SetActive(true);
        
        SteamLobby.instance.GetLobbiesList();

    }

    public void backbutton()
    {
        LobbiesButton.SetActive(true);
        HostButton.SetActive(true);


        lobbiesMenu.SetActive(false);
    }

    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result )
    {
        for (int i = 0; i < lobbyIDs.Count; i++)
        {
            if (lobbyIDs[i].m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject createdItem = Instantiate(lobbyDataItemPrefab);

                createdItem.GetComponent<LobbyDataEntry>().lobbyID = (CSteamID)lobbyIDs[i].m_SteamID;

                createdItem.GetComponent<LobbyDataEntry>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID)lobbyIDs[i].m_SteamID, "name");


                createdItem.GetComponent<LobbyDataEntry>().SetLobbyData();


                createdItem.transform.SetParent(lobbyListContent.transform);
                createdItem.transform.localScale = Vector3.one;


                ListOfLobbies.Add(createdItem);

            }
        }

    }

    public void DestroyLobbies()
    {
        foreach (GameObject lobbyitem in ListOfLobbies)
        {
            Destroy(lobbyitem);
        }

        ListOfLobbies.Clear();
    }

}
