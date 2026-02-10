using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuGameManager : MonoBehaviour
{
    public GameObject MainmenuPannel;
    public GameObject privacypannel;
    // Start is called before the first frame update
    void Start()
    {
        MainmenuPannel.SetActive(true);
        privacypannel.SetActive(false);
    }

    // Update is called once per frame
    public void Play()
    {
        MainmenuPannel.SetActive(false);
    }

    public void Exit()
    {
        Debug.Log("Game Exit Called");

        Application.Quit();
    }

    public void privacy()
    {
        privacypannel.SetActive(true);
    }

    public void accept()
    {
        privacypannel.SetActive(false);
    }

}
