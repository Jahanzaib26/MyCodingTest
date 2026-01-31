using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    //public GameObject successPanel;
    //public GameObject failPanel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Time.timeScale = 1f;
        PlayerTrigger.ResetWinner();
        //successPanel.SetActive(false);
        //failPanel.SetActive(false);
    }

    public void ShowSuccess()
    {
       // successPanel.SetActive(true);
        UnlockCursor();
    }

    public void ShowFail()
    {
      //  failPanel.SetActive(true);
        UnlockCursor();
    }

    void UnlockCursor()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ExitGame()
    {
        Debug.Log("Game Exit Called");

        Application.Quit();
    }

    public void Home()
    {
        Time.timeScale = 1f;

        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.isConnected)
            NetworkManager.singleton.StopClient();
    }


}
