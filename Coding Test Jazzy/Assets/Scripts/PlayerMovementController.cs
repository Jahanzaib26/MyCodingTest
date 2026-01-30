using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.SceneManagement;

public class PlayerMovementController : NetworkBehaviour
{

    public float speed = 5f;

    public GameObject playermodel;


    private void Start()
    {
       playermodel.SetActive(false); 
    }

    private void Update()
    {
        if(SceneManager.GetActiveScene().name == "Game")
        {
            if(playermodel.activeSelf  == false)
            {
                SetPosition();
                playermodel.SetActive (true);
            }


            if (isLocalPlayer) {

                Movement();
            } 
            
        }
    }


    public void SetPosition()
    {
        float randomX = Random.Range(-5f, 5f);
        float randomZ = Random.Range(-15f, 7f);

        // Hamesha ground ke upar rakho
        transform.position = new Vector3(randomX, 1f, randomZ);
    }


    public void Movement()
    {
        //if () return; // sirf local player control kare

        float xDirection = Input.GetAxis("Horizontal");
        float zDirection = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(xDirection, 0.0f, zDirection).normalized;

        transform.position += moveDirection * speed * Time.deltaTime;
    }



}
