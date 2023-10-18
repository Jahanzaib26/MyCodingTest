using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCard : MonoBehaviour
{
    //         At first we're trying to Animate and Flip the card              //
    public GameObject Back_Card;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        StartCoroutine(TurnFrontCard()); 
    }


    IEnumerator TurnFrontCard() {


        // Fliping the cards by rotating to 180 degrees

        for (float j = 0f; j <= 180; j += 10f) {

            transform.rotation = Quaternion.Euler(0f, j, 0f);
            yield return new WaitForSeconds(0.05f);
        }

        StartCoroutine(TurnBackCard());

    }


    IEnumerator TurnBackCard()
    {
        // Fliping the cards back to it's rotation

        yield return new WaitForSeconds(0.5f);
        for (float j = 180; j >= 0; j -= 10) {

            transform.rotation = Quaternion.Euler(0f, j, 0f);

            yield return new WaitForSeconds(0.05f);

        }

    }
    }
