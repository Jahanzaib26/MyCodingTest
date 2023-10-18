using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCard : MonoBehaviour
{
   [SerializeField] private GameController myController;


    //         At first we're trying to Animate and Flip the card              //

    private int id;



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
       

        if (myController.canRevealCard)
        {

            myController.CarDisRevealed(this);
            StartCoroutine(TurnFrontCard());

        }

    }


    

    public int card_id
    {

        get { return id; }
    }


    public void ChangeImagesSprite(int card_id, Sprite CardImg)
    {

        id = card_id;
        GetComponent<SpriteRenderer>().sprite = CardImg;


    }


    public void Un_RevealCards() {
        StartCoroutine(TurnBackCard());
    }




    IEnumerator TurnFrontCard()
    {


        // Fliping the cards by rotating to 180 degrees

        for (float j = 0f; j <= 180; j += 10f)
        {

            transform.rotation = Quaternion.Euler(0f, j, 0f);
            yield return new WaitForSeconds(0.05f);
        }


    }


    IEnumerator TurnBackCard()
    {
        // Fliping the cards back to it's rotation

        yield return new WaitForSeconds(1f);


        for (float j = 180; j >= 0; j -= 10)
        {

            transform.rotation = Quaternion.Euler(0f, j, 0f);

           

            yield return new WaitForSeconds(0.05f);

        }

    }

}