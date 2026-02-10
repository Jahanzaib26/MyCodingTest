using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovementDualSwinging : NetworkBehaviour
{
    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float swingSpeed;

    //public GameObject playermodel;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float SjumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Inventory ")]
    public KeyCode InventoryOpenClose = KeyCode.Tab;
    public KeyCode pausepannelopen = KeyCode.Escape;
    public GameObject pausepannel;
    public bool isInverntoryOpen;
    public InventoryManager inventoryManager;
    public float inventoryopenTime;

    [Header("speed Check")]
    public float speed = 10f;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;

    [Header("Camera Effects")]
    public PlayerCam cam;
    public float grappleFov = 95f;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    [Header("Swinging Settings")]
    public float maxSwingDistance = 2f;   // max distance allowed during swing
    private Vector3 swingStartPos;

    [Header("Character")]
    public GameObject playerCharacter;
    public Animator playerAnimator;

    public MovementState state;
    public enum MovementState
    {
        freeze,
        grappling,
        swinging,
        walking,
        sprinting,
        air
    }

    public bool freeze;

    public bool activeGrapple;
    public bool swinging;

    private void Start()
    {
        //playermodel.SetActive(false);
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
        pausepannel.SetActive(false);
    }


    public override void OnStartLocalPlayer()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void Update()
    {
        //if (SceneManager.GetActiveScene().name == "GameScene")
        //{
        //    if (playermodel.activeSelf == false)
        //    {
        //        SetPosition();
        //        playermodel.SetActive(true);
        //    }

        if (!isLocalPlayer) return;



        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.8f + 0.2f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler(); // State yahan set hota hai

        // --- ANIMATOR LOGIC ---
        if (playerAnimator != null)
        {
            // 1. Grounded
            playerAnimator.SetBool("isGrounded", grounded);

            // 2. Speed (for Idle/Walk blend)
            // Hum sirf "walking" state mein speed ko update karenge
            if (state == MovementState.walking)
            {
                // Flat velocity (Y-axis ke baghair)
                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
                float speed = flatVel.magnitude;

                // Speed ko walkSpeed se divide karke 0-1 value banayein (Blend Tree ke liye)
                playerAnimator.SetFloat("Speed", speed / walkSpeed);
            }
            else
            {
                // Agar swing, grapple, ya hawa mein hain, toh "walking" speed 0 hai
                playerAnimator.SetFloat("Speed", 0f);
            }
        }
        // --- END ANIMATOR LOGIC ---

        // handle drag
        if (grounded && !activeGrapple)
            rb.drag = groundDrag;
        else
            rb.drag = 0;
        TextStuff();
    }

    //public void SetPosition()
    //{
    //    // Fixed spawn position
    //    transform.position = new Vector3(236f, 38f, 234f);
    //}

    private void FixedUpdate()
    {

        if (!isLocalPlayer) return;
        MovePlayer();
    }
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");


        if (Input.GetKeyDown(pausepannelopen))
        {
            pausepannel.SetActive(true);
            // 🖱️ Cursor enable
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // ⏸️ Game pause
            //Time.timeScale = 0f;
        }


        // 🔹 Normal Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }


        // 🔹 Inventory
        inventoryopenTime += Time.deltaTime;
        if (Input.GetKey(InventoryOpenClose) && inventoryopenTime>0.3f)
        {
            inventoryopenTime = 0f;
            if (!isInverntoryOpen)
            {
                isInverntoryOpen = true;
                inventoryManager.OpenInventory(true);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                isInverntoryOpen = false;
                inventoryManager.OpenInventory(false);
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }


      


        }



        // 🔹 Swing Jump (special case)
        else if (Input.GetKeyDown(jumpKey) && swinging)
        {
            SwingJump();
        }
    }

    private void StateHandler()
    {
        // --- Animator: Override states ko reset karein ---
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("isSwinging", false);
            playerAnimator.SetBool("isGrappling", false);
        }

        // Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
        }

        // Mode - Grappling
        else if (activeGrapple)
        {
            state = MovementState.grappling;
            moveSpeed = sprintSpeed;
            if (playerAnimator != null) playerAnimator.SetBool("isGrappling", true); // 👈 Yeh add karein
        }

        // Mode - Swinging
        else if (swinging)
        {
            state = MovementState.swinging;
            moveSpeed = swingSpeed;

            // agar pehli dafa swing start hua to position save kar lo
            if (swingStartPos == Vector3.zero)
                swingStartPos = transform.position;

            if (playerAnimator != null) playerAnimator.SetBool("isSwinging", true); // 👈 Yeh add karein
        }

        //// Mode - Walking
        //else if (grounded)
        //{
        //    state = MovementState.walking;
        //    moveSpeed = walkSpeed;

        //    // agar swing khatam ho gayi to reset
        //    swingStartPos = Vector3.zero;
        //}

        // Mode - Sprinting
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;

            //// animator (optional)
            //if (playerAnimator != null)
            //    playerAnimator.SetFloat("Speed", 1.2f); // thori zyada speed feel
        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;

            // swing reset
            swingStartPos = Vector3.zero;
        }



        // Mode - Air
        else
        {
            state = MovementState.air; // 👈 Isse uncomment/add karein

            // swing khatam hone par reset
            swingStartPos = Vector3.zero; // 👈 Isse uncomment/add karein
        }
    }

    public void MovePlayer()
    {
        if (activeGrapple) return;
        if (swinging) return;

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection() * moveSpeed * speed, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * speed, ForceMode.Force);
        }

        // on ground
        else if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * speed, ForceMode.Force);

        // in air
        else if (!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * speed * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        rb.useGravity = !OnSlope();
    }

    private void SpeedControl()
    {
        if (activeGrapple) return;

        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

            // limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
    }

    public void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

        if (playerAnimator != null) playerAnimator.SetTrigger("Jump"); // 👈 Yeh add karein
    }

    // 🔥 Swing Jump (new)

    public void SwingJump()
    {
        Vector3 jumpDirection = (orientation.forward + Vector3.up).normalized;
        rb.velocity = Vector3.zero; // reset pehle
        rb.AddForce(Vector3.up * SjumpForce, ForceMode.VelocityChange);

       // if (playerAnimator != null) playerAnimator.SetTrigger("Jump"); // 👈 Yeh add karein
    }

    private void ResetJump()
    {
        readyToJump = true;
        exitingSlope = false;
    }

    private bool enableMovementOnNextTouch;
    public void JumpToPosition(Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;

        cam.DoFov(grappleFov);
    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
        cam.DoFov(85f);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<DualHooks>().CancelActiveGrapples();
        }
    }

    private bool OnSlope()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }

    private Vector3 GetSlopeMoveDirection()
    {
        return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity)
            + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));

        return velocityXZ + velocityY;
    }

    #region Text & Debugging

    public TextMeshProUGUI text_speed;
    public TextMeshProUGUI text_mode;
    private void TextStuff()
    {
        //Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        //if (OnSlope())
        //    text_speed.SetText("Speed: " + Round(rb.velocity.magnitude, 1) + " / " + Round(moveSpeed, 1));

        //else
        //    text_speed.SetText("Speed: " + Round(flatVel.magnitude, 1) + " / " + Round(moveSpeed, 1));

        //text_mode.SetText(state.ToString());
    }

    public static float Round(float value, int digits)
    {
        float mult = Mathf.Pow(10.0f, (float)digits);
        return Mathf.Round(value * mult) / mult;
    }

    #endregion

}
