using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{     /// <summary>
/// Now we have to make row and columns for the other cards to instantiate and also have to choose the distance betwenn each card Vertically and horizontally 
/// </summary>


    public int cardRows = 2;    // Rows of the grid used for cards to instantiate
    public int cardColumns = 3;  // columns in the grid used for cards to instantiate
    public float offSet_DisX = 4f;    // distance b/w cards on X Axis
    public float offSet_DisY = 5f;    // distance b/w cards on Y Axis




    // Taking the cards that's gonna be revealed on turn
    MainCard first_card_revealed;
    MainCard second_card_revealed;



    [SerializeField]  Sprite[] card_images;  // sprites that are going to be instantiated on the screen
    [SerializeField] MainCard Org_Main_Card;  // this wil be our main card object that has the main card script attached to it wo we can get the references from that script

    //Sounds

    public AudioSource win, lose, match, mis_match;


    // Score for matching cards
    public int matchScore = 0;
    public int amount = 0;

    [SerializeField]   private TextMesh scoreTxt;

    // Start is called before the first frame update
    void Start()
    {

        matchScore = PlayerPrefs.GetInt("Player Score");
        Vector3 MainCardStartPos = Org_Main_Card.transform.position;   // this is the position of our main Orig card and all the other cards will be offset to this card
        int[] nums = { 0, 0, 1, 1, 2, 2, 3, 3, 4, 4, 5, 5, 6, 6, 7, 7, 8, 8, 9, 9 };
        nums = RandomShuffleArray(nums); // This function will be used for randomly shuffling the cards

        for (int j = 0; j < cardColumns; j++) {

            for (int l = 0; l < cardRows; l++) {

                MainCard myCard;
                if (j == 0 && l == 0)
                {

                    myCard = Org_Main_Card;

                }
                else {

                    myCard = Instantiate(Org_Main_Card) as MainCard;

                   
                }

                //  getting the id num of the card image/sprite that's gonna be later used for matching them 

                int index = l * cardColumns + j;
                int card_id = nums[index];
                myCard.ChangeImagesSprite(card_id, card_images[card_id]);


                // setting the pos on X and Y Axis from the offset of our Main card that's already been in the Hierarchy

                float posX_axis = (offSet_DisX * j) + MainCardStartPos.x;
                float posY_axis = (offSet_DisY * l) + MainCardStartPos.y;
                myCard.transform.position = new Vector3(posX_axis,posY_axis,MainCardStartPos.z);

            
            }
        
        
        }

    }


    private int[] RandomShuffleArray(int[] nums) {    // Randomly shuffling the cards on screen  by making the clons of the original card

        int[] myArray = nums.Clone() as int[];
        for (int j = 0; j < myArray.Length; j++) {

            int temp = myArray[j];
            int r = Random.RandomRange(j,myArray.Length);
            myArray[j] = myArray[r];
            myArray[r] = temp;
        
        }
        return myArray;
    
    }
 


    public bool canRevealCard
    {
        get { return second_card_revealed == null; }
    
    }


    public void CarDisRevealed(MainCard myCard) {

        if (first_card_revealed == null) {

            first_card_revealed = myCard;


        }
        else {

            second_card_revealed = myCard;
            StartCoroutine(CheckMatch());
        }
    
    }


    IEnumerator CheckMatch() {

        if (first_card_revealed.card_id == second_card_revealed.card_id)
        {

            match.Play();
            amount = amount + 1;
            matchScore += amount;
            PlayerPrefs.SetInt("Player Score" , matchScore);
          

            Debug.Log("Win Win");

            yield return new WaitForSeconds(1f);

            first_card_revealed.gameObject.SetActive(false);
            second_card_revealed.gameObject.SetActive(false);


        }
        else {


            mis_match.Play();

            yield return new WaitForSeconds(0.5f);
            first_card_revealed.Un_RevealCards();
            second_card_revealed.Un_RevealCards();
        }


        first_card_revealed = null;
        second_card_revealed = null;
    
    
    }

    // Update is called once per frame
    void Update()
    {

        scoreTxt.text = PlayerPrefs.GetInt("Player Score").ToString();
    }
}
