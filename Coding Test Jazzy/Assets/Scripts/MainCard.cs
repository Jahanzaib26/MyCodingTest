using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCard : MonoBehaviour
{
    //         At first we're trying to Animate and Flip the card              //
    public GameObject Back_Card;
    bool Allow_to_turn, front_Face;


    private int id;
    // Start is called before the first frame update
    void Start()
    {
        Allow_to_turn = true;
        front_Face = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnMouseDown()
    {
        // StartCoroutine(TurnFrontCard()); 
        if (Allow_to_turn) {

            StartCoroutine(TurnTheCards());
        
        }

    }


    IEnumerator TurnTheCards() {
        Allow_to_turn = false;
        if (!front_Face)
        {

            for (float j = 0f; j <= 180; j += 10f)
            {

                transform.rotation = Quaternion.Euler(0f, j, 0f);
                yield return new WaitForSeconds(0.05f);
            }


        }


        else if (front_Face)
        {

            for (float j = 180; j >= 0; j -= 10)
            {

                transform.rotation = Quaternion.Euler(0f, j, 0f);

                yield return new WaitForSeconds(0.05f);

            }
        }



        Allow_to_turn = true;
        front_Face = !front_Face;  

        }


    public int card_id {

        get { return id; }
    }


    public void ChangeImagesSprite(int card_id, Sprite CardImg) {

        id = card_id;
        GetComponent<SpriteRenderer>().sprite = CardImg;
    
    
    }













    }







    //IEnumerator TurnFrontCard() {


    //    // Fliping the cards by rotating to 180 degrees

    //    for (float j = 0f; j <= 180; j += 10f) {

    //        transform.rotation = Quaternion.Euler(0f, j, 0f);
    //        yield return new WaitForSeconds(0.05f);
    //    }

    //    StartCoroutine(TurnBackCard());

    //}


    //IEnumerator TurnBackCard()
    //{
    //    // Fliping the cards back to it's rotation

    //    yield return new WaitForSeconds(0.5f);
    //    for (float j = 180; j >= 0; j -= 10) {

    //        transform.rotation = Quaternion.Euler(0f, j, 0f);

    //        yield return new WaitForSeconds(0.05f);

    //    }

    //}
    
