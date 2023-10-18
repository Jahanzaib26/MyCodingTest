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


   [SerializeField]  Sprite[] card_images;  // sprites that are going to be instantiated on the screen
    [SerializeField] MainCard Org_Main_Card;  // this wil be our main card object that has the main card script attached to it wo we can get the references from that script
    // Start is called before the first frame update
    void Start()
    {
        Vector3 MainCardStartPos = Org_Main_Card.transform.position;   // this is the position of our main Orig card and all the other cards will be offset to this card
        int[] nums = { 0, 0, 1, 1, 2, 2, 3, 3 };
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
    // Update is called once per frame
    void Update()
    {
        
    }
}