using Mirror.BouncyCastle.Asn1.Mozilla;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuGameManager : MonoBehaviour
{

    private int[] qualityLevels = { 1, 3, 5 };
    // 0 = Low
    // 1 = Medium
    // 2 = Ultra

    public GameObject MainmenuPannel;
    public GameObject privacypannel;
    public GameObject Exitpannel;
    public GameObject settingspannel;
    public GameObject soundon;
    public GameObject soundof;
    public GameObject musicon;
    public GameObject musicof;

    [Header("Audio Sources")]
    public AudioSource[] soundSources;   // SFX ke liye
    public AudioSource[] musicSources;   // Music ke liye

    public GameObject low;
    public GameObject medium;
    public GameObject high;

    private int currentIndex = 2; // Default Ultra

    // Start is called before the first frame update
    void Start()
    {
        ApplyQuality();
        MainmenuPannel.SetActive(true);
        privacypannel.SetActive(false);
        Exitpannel.SetActive(false);
        soundon.SetActive(true);
        musicon.SetActive(true);
        low.SetActive(false);
        medium.SetActive(false);
        high.SetActive(true);
        settingspannel.SetActive(false);

    }

    void ApplyQuality()
    {
        int unityIndex = qualityLevels[currentIndex];
        QualitySettings.SetQualityLevel(unityIndex);

        low.SetActive(currentIndex == 0);
        medium.SetActive(currentIndex == 1);
        high.SetActive(currentIndex == 2);
    }

    // Update is called once per frame
    public void Play()
    {
        MainmenuPannel.SetActive(false);
    }

    public void Exit()
    {
        Exitpannel.SetActive(true);
    }
    public void Settings()
    {
        settingspannel.SetActive(true);
    }

    public void privacy()
    {
        privacypannel.SetActive(true);
    }

    public void accept()
    {
        privacypannel.SetActive(false);
    }

    public void ok()
    {
        Debug.Log("Game Exit Called");

        Application.Quit();
    }

    public void no()
    {
        Exitpannel.SetActive(false);
    }

    public void SoundON()
    {
        soundon.SetActive(false);
        soundof.SetActive(true);

        foreach (AudioSource source in soundSources)
        {
            source.mute = true;   // sound OFF
        }
    }

    public void SoundOF()
    {
        soundon.SetActive(true);
        soundof.SetActive(false);

        foreach (AudioSource source in soundSources)
        {
            source.mute = false;   // sound ON
        }
    }

    public void MusicON()
    {
        musicon.SetActive(false);
        musicof.SetActive(true);

        foreach (AudioSource source in musicSources)
        {
            source.mute = true;   // music OFF
        }
    }

    public void MusicOF()
    {
        musicon.SetActive(true);
        musicof.SetActive(false);

        foreach (AudioSource source in musicSources)
        {
            source.mute = false;   // music ON
        }
    }

    public void leftarrow()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            ApplyQuality();
        }
    }



    public void Rightarrow()
    {
        if (currentIndex < qualityLevels.Length - 1)
        {
            currentIndex++;
            ApplyQuality();
        }
    }


    public void save()
    {
        settingspannel.SetActive(false);
    }

}
