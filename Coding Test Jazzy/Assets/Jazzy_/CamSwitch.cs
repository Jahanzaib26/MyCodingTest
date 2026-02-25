using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamSwitch : MonoBehaviour
{
    public GameObject cam1, cam2,birds,player,fade;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Movie());   
    }

    // Update is called once per frame
    IEnumerator Movie() {


        yield return new WaitForSeconds(3.2f);
        cam1.GetComponent<Animator>().enabled = true;
        yield return new WaitForSeconds(15f);
        player.SetActive(true);
        birds.SetActive(false);
        cam1.SetActive(false);
        cam2.SetActive(true);
        yield return new WaitForSeconds(16f);
        fade.SetActive(true);
    }
}
