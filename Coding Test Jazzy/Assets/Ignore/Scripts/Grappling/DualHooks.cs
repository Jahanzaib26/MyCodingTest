using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;

public class DualHooks : NetworkBehaviour
{
    public Transform leftHoldPoint;
    public Transform rightHoldPoint;

    public GameObject leftHeldObject;
    public GameObject rightHeldObject;


    public GameObject lft_hand_mesh;
    public GameObject rt_hand_mesh;
    //public static DualHooks instance;

    [Header("References")]
    public List<LineRenderer> lineRenderers;
    public List<Transform> gunTips;
    public Transform cam;
    public Transform player;
    public LayerMask whatIsGrappleable;
    public PlayerMovementDualSwinging pm;
    private int lastSwingFrame = -1;


    [Header("Swinging")]
    [Range(5f, 15f)]
    public float maxSwingDistance = 0.1f;   // pehle 25f tha
    private List<Vector3> swingPoints;
    private List<SpringJoint> joints;

    [Header("Grappling")]
    public float maxGrappleDistance;
    public float grappleDelayTime;
    public float overshootYAxis;

    private List<bool> grapplesActive;

    [Header("Cooldown")]
    public float grapplingCd;
    private float grapplingCdTimer;

    [Header("OdmGear")]
    public Transform orientation;
    public Rigidbody rb;
    public float horizontalThrustForce;
    public float forwardThrustForce;
    public float extendCableSpeed;

    [Header("Prediction")]
    public List<RaycastHit> predictionHits;
    public List<Transform> predictionPoints;
    public float predictionSphereCastRadius;

    [Header("Input")]
    public KeyCode swingKey1 = KeyCode.Mouse0;
    public KeyCode swingKey2 = KeyCode.Mouse1;


    [Header("DualSwinging")]
    public int amountOfSwingPoints = 2;
    public List<Transform> pointAimers;
    public List<bool> swingsActive;
    public PlayerMovementDualSwinging playerMovementDualSwinging;


    [Header("Stamina System")]
    public float maxStamina = 100f;
    public float drainRate = 20f; // stamina per second
    public float rechargeRate = 15f; // per second recharge
    [Header("Combined Stamina UI")]
    [Header("Stamina UI")]
    public Image leftStaminaBar;
    public Image rightStaminaBar;


    private float leftStamina;
    private float rightStamina;


    [Header("Pickup System")]
    public float pickupDistance = 5f;
    public LayerMask pickupLayerMask; // Isse Inspector se set karna hoga
    public GameObject leftHandObject = null;
    public GameObject rightHandObject = null;

    private Camera mainCamera;

    [Header("Public InventoryManager")]
    public InventoryManager inventoryManager;

    [Header("Hands")]
    public GameObject leftHand;
    public Animator leftHandAnimator;
    public GameObject rightHand;
    public Animator rightHandAnimator;

    private Quaternion leftGrabRotation;
    private Quaternion rightGrabRotation;

    [Header("Hand Animation & Positioning")]
    public Transform leftHandRestPos;
    public Transform rightHandRestPos;
    public float handReturnSpeed = 5f; // Lerp speed back to rest
    private bool leftReturning = false;
    private bool rightReturning = false;

    [Header("Hand Grab Positioning")]
    public float handAttachSpeed = 10f;
    public float handRotationAttachSpeed = 10f;
    public Vector3 leftGrabOffset = new Vector3(0.1f, 0f, 0f);
    public Vector3 rightGrabOffset = new Vector3(-0.1f, 0f, 0f);

    private bool leftGrabbing = false;
    private bool rightGrabbing = false;
    private Vector3 leftGrabTarget;
    private Vector3 rightGrabTarget;

    private Transform leftHandParent;
    private Transform rightHandParent;


    private void Start()
    {
        //instance = this;

        ListSetup();
        leftStamina = maxStamina;
        rightStamina = maxStamina;

        mainCamera = cam.GetComponent<Camera>();

        if (leftHand != null)
            leftHandParent = leftHand.transform.parent;
        if (rightHand != null)
            rightHandParent = rightHand.transform.parent;
    }


    private void ListSetup()
    {
        predictionHits = new List<RaycastHit>();

        swingPoints = new List<Vector3>();
        joints = new List<SpringJoint>();

        swingsActive = new List<bool>();
        grapplesActive = new List<bool>();

        currentGrapplePositions = new List<Vector3>();

        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            predictionHits.Add(new RaycastHit());
            joints.Add(null);
            swingPoints.Add(Vector3.zero);
            swingsActive.Add(false);
            grapplesActive.Add(false);
            currentGrapplePositions.Add(Vector3.zero);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        MyInput();
        CheckForSwingPoints();
        HandleAutoGrabOnHold();


        if (joints[0] != null || joints[1] != null) OdmGearMovement();

        HandleStamina();
        HandleHandGrabPositioning();
        HandleHandReturn();

        if (grapplingCdTimer > 0)
            grapplingCdTimer -= Time.deltaTime;
    }

    private void LateUpdate()
    {
        if (!isLocalPlayer) return;
        DrawRope();
    }

    private void HandleAutoGrabOnHold()
    {
        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            bool buttonHeld =
                (i == 0 && Input.GetKey(swingKey1)) ||
                (i == 1 && Input.GetKey(swingKey2));

            // Agar button hold hai
            // Aur swing already active nahi hai
            // Aur valid prediction point hai
            if (buttonHeld &&
                !swingsActive[i] &&
                predictionHits[i].point != Vector3.zero &&
                Vector3.Distance(player.position, predictionHits[i].point) <= maxSwingDistance)
            {
                StartSwing(i);
            }
        }
    }


    private void HandleStamina()
    {
        bool leftActive = swingsActive[0];
        bool rightActive = swingsActive[1];
        bool bothActive = leftActive && rightActive;

        float effectiveDrain = drainRate;

        // if both hooked to the same point (within small distance)
        if (bothActive && Vector3.Distance(swingPoints[0], swingPoints[1]) < 1f)
            effectiveDrain *= 0.7f; // slower drain when both on same point

        // Left Hand
        if (leftActive)
        {
            leftStamina -= effectiveDrain * Time.deltaTime;
            if (leftStamina <= 0f)
            {
                StopSwing(0);
                leftStamina = 0f;
            }
        }
        else
        {
            leftStamina += rechargeRate * Time.deltaTime;
            leftStamina = Mathf.Min(leftStamina, maxStamina);
        }

        // Right Hand
        if (rightActive)
        {
            rightStamina -= effectiveDrain * Time.deltaTime;
            if (rightStamina <= 0f)
            {
                StopSwing(1);
                rightStamina = 0f;
            }
        }
        else
        {
            rightStamina += rechargeRate * Time.deltaTime;
            rightStamina = Mathf.Min(rightStamina, maxStamina);
        }
        UpdateStaminaUI();

    }

    private void UpdateStaminaUI()
    {


        float leftPercent = leftStamina / maxStamina;

        if (leftPercent <= 0.3f)
            leftStaminaBar.color = Color.red;
        else if (leftPercent <= 0.6f)
            leftStaminaBar.color = Color.yellow;
        else
            leftStaminaBar.color = Color.green;


        // RIGHT
        float rightPercent = rightStamina / maxStamina;

        if (rightPercent <= 0.3f)
            rightStaminaBar.color = Color.red;
        else if (rightPercent <= 0.6f)
            rightStaminaBar.color = Color.yellow;
        else
            rightStaminaBar.color = Color.green;




        // LEFT HAND
        if (leftStaminaBar != null)
        {
            leftStaminaBar.gameObject.SetActive(swingsActive[0]);

            if (swingsActive[0])
                leftStaminaBar.fillAmount = leftStamina / maxStamina;
        }

        // RIGHT HAND
        if (rightStaminaBar != null)
        {
            rightStaminaBar.gameObject.SetActive(swingsActive[1]);

            if (swingsActive[1])
                rightStaminaBar.fillAmount = rightStamina / maxStamina;
        }
    }





    private void MyInput()
    {
        // --- Left Hand Input (Mouse 0) ---
        if (Input.GetKeyDown(swingKey1))
        {
            if (inventoryManager.GetSelectedLeftItem() == null)
            {
                // Haath khaali hai -> Pickup ki koshish karo
                AttemptPickup(0);

                // Haath bhara hai -> Grapple/Swing karo
                if (Input.GetKey(KeyCode.LeftShift))
                    StartGrapple(0);
                else
                    StartSwing(0);

            }

        }

        // --- Right Hand Input (Mouse 1) ---
        if (Input.GetKeyDown(swingKey2))
        {
            if (inventoryManager.GetSelectedRightItem() == null)
            {
                // Haath khaali hai -> Pickup ki koshish karo
                AttemptPickup(1);

                // Haath bhara hai -> Grapple/Swing karo
                if (Input.GetKey(KeyCode.LeftShift))
                    StartGrapple(1);
                else
                    StartSwing(1);
            }

        }

        // stopping is always possible
        if (Input.GetKeyUp(swingKey1)) StopSwing(0);
        if (Input.GetKeyUp(swingKey2)) StopSwing(1);


        // --- [BONUS] Drop Logic (Haath khaali karne ke liye) ---
        // G = Drop Left Hand, H = Drop Right Hand
        if (Input.GetKeyDown(KeyCode.G)) DropItem(0);
        if (Input.GetKeyDown(KeyCode.H)) DropItem(1);
    }

    private void CheckForSwingPoints()
    {
        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            if (swingsActive[i]) { /* Do Nothing */ }
            else
            {
                RaycastHit sphereCastHit;
                Physics.SphereCast(
                    pointAimers[i].position,
                    predictionSphereCastRadius,
                    pointAimers[i].forward,
                    out sphereCastHit,
                    maxSwingDistance,
                    whatIsGrappleable
                );

                RaycastHit raycastHit;
                Physics.Raycast(
                    cam.position,
                    cam.forward,
                    out raycastHit,
                    maxSwingDistance,
                    whatIsGrappleable
                );

                Vector3 realHitPoint;

                // Option 1 - Direct Hit
                if (raycastHit.point != Vector3.zero)
                    realHitPoint = raycastHit.point;

                // Option 2 - Indirect (predicted) Hit
                else if (sphereCastHit.point != Vector3.zero)
                    realHitPoint = sphereCastHit.point;

                // Option 3 - Miss
                else
                    realHitPoint = Vector3.zero;

                // ✅ Final Clamp check with maxSwingDistance
                if (realHitPoint != Vector3.zero &&
                    Vector3.Distance(player.position, realHitPoint) <= maxSwingDistance)
                {
                    float activationDistance = 3f; // apna custom distance
                    float playerToPointDist = Vector3.Distance(player.position, realHitPoint);

                    if (playerToPointDist <= activationDistance)
                    {
                        predictionPoints[i].gameObject.SetActive(true);
                        predictionPoints[i].position = realHitPoint;

                        // predictionHit ko bhi set karo
                        predictionHits[i] = raycastHit.point == Vector3.zero ? sphereCastHit : raycastHit;
                    }
                    else
                    {
                        predictionPoints[i].gameObject.SetActive(false);
                        predictionHits[i] = new RaycastHit(); // reset
                    }
                }
                else
                {
                    predictionPoints[i].gameObject.SetActive(false);
                    predictionHits[i] = new RaycastHit(); // reset
                }


            }
        }
    }
    #region Swinging

    private void StartSwing(int swingIndex)
    {

        // 🚫 If another swing already started THIS FRAME → block
        if (lastSwingFrame == Time.frameCount)
            return;

        lastSwingFrame = Time.frameCount;
        if (predictionHits[swingIndex].point == Vector3.zero) return;
        float dist = Vector3.Distance(player.position, predictionHits[swingIndex].point);
        if (dist > maxSwingDistance) return;

        // deactivate grapple
        CancelActiveGrapples();
        pm.ResetRestrictions();
        pm.swinging = true;
        swingsActive[swingIndex] = true;

        swingPoints[swingIndex] = predictionHits[swingIndex].point;
        joints[swingIndex] = player.gameObject.AddComponent<SpringJoint>();
        joints[swingIndex].autoConfigureConnectedAnchor = false;
        joints[swingIndex].connectedAnchor = swingPoints[swingIndex];

        float distanceFromPoint = Vector3.Distance(player.position, swingPoints[swingIndex]);

        // --- Phase 1: Immediately pull close ---
        joints[swingIndex].maxDistance = distanceFromPoint * 0.25f;
        joints[swingIndex].minDistance = distanceFromPoint * 0.2f;
        joints[swingIndex].spring = 20f;
        joints[swingIndex].damper = 10f;
        joints[swingIndex].massScale = 2f;
        Vector3 pullDir = (swingPoints[swingIndex] - player.position).normalized;

        // Reset small opposing velocity
        rb.velocity = Vector3.Project(rb.velocity, pullDir);

        // Add strong initial impulse
        rb.AddForce(pullDir * 12f, ForceMode.VelocityChange);

        lineRenderers[swingIndex].positionCount = 2;
        currentGrapplePositions[swingIndex] = gunTips[swingIndex].position;

        // --- Attach hand to grapple point visually ---
        if (swingIndex == 0)
        {
            leftGrabTarget = swingPoints[0] + leftGrabOffset;
            leftGrabbing = true;
            leftReturning = false;

            // --- YAHAN SE BADLAAV SHURU ---
            Vector3 dir = (leftGrabTarget - leftHand.transform.position).normalized;
            if (dir != Vector3.zero)
            {
                // Haath ka Z-axis (Forward) target ki taraf ho,
                // aur Y-axis (Up) dunya ke Y-axis se align ho (seedha).
                leftGrabRotation = Quaternion.LookRotation(dir, Vector3.up);

                // Rotation ko foran apply karein
                leftHand.transform.rotation = leftGrabRotation;
            }
            // --- YAHAN TAK BADLAAV ---

            // Detach left hand from PlayerCam
            leftHand.transform.SetParent(null, true);

            leftHandAnimator.SetBool("isGrabbing", true);
            predictionPoints[0].gameObject.SetActive(false);
        }
        else if (swingIndex == 1)
        {
            rightGrabTarget = swingPoints[1] + rightGrabOffset;
            rightGrabbing = true;
            rightReturning = false;

            // --- YAHAN SE BADLAAV SHURU ---
            Vector3 dir = (rightGrabTarget - rightHand.transform.position).normalized;
            if (dir != Vector3.zero)
            {
                // Haath ka Z-axis (Forward) target ki taraf ho,
                // aur Y-axis (Up) dunya ke Y-axis se align ho (seedha).
                rightGrabRotation = Quaternion.LookRotation(dir, Vector3.up);

                // Rotation ko foran apply karein
                rightHand.transform.rotation = rightGrabRotation;
            }
            // --- YAHAN TAK BADLAAV ---

            // Detach right hand from PlayerCam
            rightHand.transform.SetParent(null, true);

            rightHandAnimator.SetBool("isGrabbing", true);
            predictionPoints[1].gameObject.SetActive(false);
        }


        // --- After short delay, loosen rope for freedom ---
        //StartCoroutine(RelaxRopeAfterAttach(swingIndex, distanceFromPoint));
    }


    public void StopSwing(int swingIndex)
    {
        pm.swinging = false;
        swingsActive[swingIndex] = false;

        if (joints[swingIndex] != null)
            Destroy(joints[swingIndex]);

        if (swingIndex == 0)
        {
            leftGrabbing = false;
            leftReturning = true;
            leftHandAnimator.SetBool("isGrabbing", false);
            predictionPoints[0].gameObject.SetActive(true);

            // Re-parent to PlayerCam
            leftHand.transform.SetParent(leftHandParent, true);
        }
        else if (swingIndex == 1)
        {
            rightGrabbing = false;
            rightReturning = true;
            rightHandAnimator.SetBool("isGrabbing", false);
            predictionPoints[1].gameObject.SetActive(true);

            // Re-parent to PlayerCam
            rightHand.transform.SetParent(rightHandParent, true);
        }

        // Enable prediction again after release
        predictionPoints[swingIndex].gameObject.SetActive(true);
    }




    private void HandleHandGrabPositioning()
    {
        // --- Left Hand ---
        if (leftGrabbing && leftHand != null)
        {
            // Move toward grab point
            leftHand.transform.position = Vector3.Lerp(
                leftHand.transform.position,
                leftGrabTarget,
                Time.deltaTime * handAttachSpeed
            );

            // Face the grab point (Z forward)
            //Vector3 dir = (leftGrabTarget - leftHand.transform.position).normalized;
            //if (dir != Vector3.zero)
            //{
            //    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            //    leftHand.transform.rotation = targetRot;
            //    //leftHand.transform.rotation = Quaternion.Slerp(
            //    //    leftHand.transform.rotation,
            //    //    targetRot,
            //    //    Time.deltaTime * handRotationAttachSpeed
            //    //);
            //}
        }

        // --- Right Hand ---
        if (rightGrabbing && rightHand != null)
        {
            // Move toward grab point
            rightHand.transform.position = Vector3.Lerp(
                rightHand.transform.position,
                rightGrabTarget,
                Time.deltaTime * handAttachSpeed
            );

            //// Face the grab point (Z forward)
            //Vector3 dir = (rightGrabTarget - rightHand.transform.position).normalized;
            //if (dir != Vector3.zero)
            //{
            //    Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            //    rightHand.transform.rotation = targetRot;
            //}
        }
    }


    private void HandleHandReturn()
    {
        if (leftReturning && leftHand != null && leftHandRestPos != null)
        {
            leftHand.transform.position = Vector3.Lerp(
                leftHand.transform.position,
                leftHandRestPos.position,
                Time.deltaTime * handReturnSpeed
            );
            leftHand.transform.rotation = Quaternion.Lerp(
                leftHand.transform.rotation,
                leftHandRestPos.rotation,
                Time.deltaTime * handReturnSpeed
            );

            if (Vector3.Distance(leftHand.transform.position, leftHandRestPos.position) < 0.05f)
                leftReturning = false;
        }

        if (rightReturning && rightHand != null && rightHandRestPos != null)
        {
            rightHand.transform.position = Vector3.Lerp(
                rightHand.transform.position,
                rightHandRestPos.position,
                Time.deltaTime * handReturnSpeed
            );
            rightHand.transform.rotation = Quaternion.Lerp(
                rightHand.transform.rotation,
                rightHandRestPos.rotation,
                Time.deltaTime * handReturnSpeed
            );

            if (Vector3.Distance(rightHand.transform.position, rightHandRestPos.position) < 0.05f)
                rightReturning = false;
        }
    }





    #endregion

    #region Grappling

    private void StartGrapple(int grappleIndex)
    {
        if (grapplingCdTimer > 0) return;

        CancelActiveSwings();
        CancelAllGrapplesExcept(grappleIndex);

        // Case 1 - target point found
        if (predictionHits[grappleIndex].point != Vector3.zero)
        {
            Invoke(nameof(DelayedFreeze), 0.05f);

            grapplesActive[grappleIndex] = true;

            swingPoints[grappleIndex] = predictionHits[grappleIndex].point;

            StartCoroutine(ExecuteGrapple(grappleIndex));
        }

        // Case 2 - target point not found
        else
        {
            swingPoints[grappleIndex] = cam.position + cam.forward * maxGrappleDistance;

            StartCoroutine(StopGrapple(grappleIndex, grappleDelayTime));
        }

        lineRenderers[grappleIndex].positionCount = 2;
        currentGrapplePositions[grappleIndex] = gunTips[grappleIndex].position;
    }
    public bool IsAnyHookActive()
    {
        foreach (var lr in lineRenderers)
        {
            if (lr.enabled) return true;  // agar koi rope visible hai => active hai
        }
        return false;
    }

    private void DelayedFreeze()
    {
        pm.freeze = true;
    }

    private IEnumerator ExecuteGrapple(int grappleIndex)
    {
        yield return new WaitForSeconds(grappleDelayTime);

        pm.freeze = false;

        Vector3 lowestPoint = new Vector3(transform.position.x, transform.position.y - 1f, transform.position.z);

        float grapplePointRelativeYPos = swingPoints[grappleIndex].y - lowestPoint.y;
        float highestPointOnArc = grapplePointRelativeYPos + overshootYAxis;

        if (grapplePointRelativeYPos < 0) highestPointOnArc = overshootYAxis;

        pm.JumpToPosition(swingPoints[grappleIndex], highestPointOnArc);
    }

    public IEnumerator StopGrapple(int grappleIndex, float delay = 0f)
    {
        yield return new WaitForSeconds(delay);

        pm.freeze = false;

        pm.ResetRestrictions();

        grapplesActive[grappleIndex] = false;

        grapplingCdTimer = grapplingCd;
    }

    #endregion

    #region OdmGear

    private Vector3 pullPoint;
    private void OdmGearMovement()
    {
        if (swingsActive[0] && !swingsActive[1]) pullPoint = swingPoints[0];
        if (swingsActive[1] && !swingsActive[0]) pullPoint = swingPoints[1];
        // get midpoint if both swing points are active
        if (swingsActive[0] && swingsActive[1])
        {
            Vector3 dirToGrapplePoint1 = swingPoints[1] - swingPoints[0];
            pullPoint = swingPoints[0] + dirToGrapplePoint1 * 0.5f;
        }
        // right
        if (Input.GetKey(KeyCode.D)) rb.AddForce(orientation.right * horizontalThrustForce * Time.deltaTime);
        // left
        if (Input.GetKey(KeyCode.A)) rb.AddForce(-orientation.right * horizontalThrustForce * Time.deltaTime);
        // forward
        if (Input.GetKey(KeyCode.W))
        {
            if (swingsActive[0] || swingsActive[1])
            {
                // Wall stick avoid system
                RaycastHit hit;
                float checkDist = 0.8f;
                Vector3 origin = player.position + Vector3.up * 0.2f;

                if (Physics.Raycast(origin, orientation.forward, out hit, checkDist, whatIsGrappleable))
                {
                    // Agar wall bahut close hai -> upward + wall ke normal me push
                    Vector3 antiStickDir = (Vector3.up + hit.normal * 0.7f).normalized;
                    rb.AddForce(antiStickDir * forwardThrustForce * Time.deltaTime, ForceMode.Acceleration);
                }
                else
                {
                    // Wall close nahi -> sirf upward force
                    rb.AddForce(Vector3.up * forwardThrustForce * Time.deltaTime, ForceMode.Acceleration);
                }
            }
            else
            {
                // Normal forward move jab swing active nahi hai
                rb.AddForce(orientation.forward * forwardThrustForce * Time.deltaTime, ForceMode.Acceleration);
            }
        }
        // backward
        //if (Input.GetKey(KeyCode.S)) rb.AddForce(-orientation.forward * forwardThrustForce * Time.deltaTime);
        // shorten cable
        // shorten cable / swing jump
        // Swing Jump (space press karte hi)
        // Swing Jump (space press karte hi)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 🔥 If ANY swing is active, always jump
            if (swingsActive[0] || swingsActive[1])
            {
                pm.SwingJump();

                // Detach both swings
                StopSwing(0);
                StopSwing(1);

                // Optional: regrab logic
                StartCoroutine(AutoRegrabAfterJump());
            }
        }





        //playerMovementDualSwinging.Jump();
        //Vector3 directionToPoint = pullPoint - transform.position;
        //rb.AddForce(directionToPoint.normalized * forwardThrustForce * Time.deltaTime);

        //calculate the distance to the grapplePoint
        //float distanceFromPoint = Vector3.Distance(transform.position, pullPoint);

        // the distance grapple will try to keep from grapple point.
        //UpdateJoints(distanceFromPoint);

        //print("shorten " + Time.time);

        // extend cable
        if (Input.GetKey(KeyCode.S))
        {
            // calculate the distance to the grapplePoint
            float extendedDistanceFromPoint = Vector3.Distance(transform.position, pullPoint);

            // the distance grapple will try to keep from grapple point.
            UpdateJoints(extendedDistanceFromPoint);

            print("extend " + Time.time);
        }


    }


    private IEnumerator AutoRegrabAfterJump()
    {
        // ⏳ thoda sa wait taake ropes properly detach ho jayein
        yield return new WaitForSeconds(0.1f);

        // 🚀 jab tak player ground pe nahi aata, tab tak regrab check karta rahe
        while (!pm.grounded)
        {
            bool leftHeld = Input.GetKey(swingKey1);
            bool rightHeld = Input.GetKey(swingKey2);

            for (int i = 0; i < amountOfSwingPoints; i++)
            {
                // agar button release ho gaya to skip
                if ((i == 0 && !leftHeld) || (i == 1 && !rightHeld))
                    continue;

                // agar valid swing point mil gaya aur distance sahi hai
                if (predictionHits[i].point != Vector3.zero &&
                    Vector3.Distance(player.position, predictionHits[i].point) <= maxSwingDistance)
                {
                    // same point pe dobara attach na ho
                    if (swingPoints[i] != predictionHits[i].point)
                    {
                        StartSwing(i);
                        yield break; // attach ho gaya to loop khatam
                    }
                }
            }

            yield return null; // next frame tak wait
        }
    }


    private void UpdateJoints(float distanceFromPoint)
    {
        for (int i = 0; i < joints.Count; i++)
        {
            if (joints[i] != null)
            {
                joints[i].maxDistance = distanceFromPoint * 0.3f;
                joints[i].minDistance = distanceFromPoint * 0.2f;
            }
        }
    }
    #endregion

    #region CancleAbilities

    public void CancelActiveGrapples()
    {
        StartCoroutine(StopGrapple(0));
        StartCoroutine(StopGrapple(1));
    }

    private void CancelAllGrapplesExcept(int grappleIndex)
    {
        for (int i = 0; i < amountOfSwingPoints; i++)
            if (i != grappleIndex) StartCoroutine(StopGrapple(i));
    }

    private void CancelActiveSwings()
    {
        StopSwing(0);
        StopSwing(1);
    }

    #endregion

    #region Visualisation

    private List<Vector3> currentGrapplePositions;

    private void DrawRope()
    {
        for (int i = 0; i < amountOfSwingPoints; i++)
        {
            // if not grappling, don't draw rope
            if (!grapplesActive[i] && !swingsActive[i])
            {
                lineRenderers[i].positionCount = 0;
            }
            else
            {
                currentGrapplePositions[i] = Vector3.Lerp(currentGrapplePositions[i], swingPoints[i], Time.deltaTime * 8f);

                lineRenderers[i].SetPosition(0, gunTips[i].position);
                lineRenderers[i].SetPosition(1, currentGrapplePositions[i]);
            }
        }
    }

    #endregion


    private void AttemptPickup(int handIndex)
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupDistance, pickupLayerMask))
        {
            Item itemOnObject = hit.collider.GetComponent<ItemInfo>().itemsToPickup;

            if (itemOnObject != null)
            {
                InventorySlot targetSlot = inventoryManager.slots[handIndex];

                // 🔥 CAPTURE UI ITEM
                InventoryItem newUIItem = inventoryManager.SpwanItem(itemOnObject, targetSlot);

                GameObject pickedObject = hit.collider.gameObject;

                // 🔥 LINK UI ITEM TO WORLD OBJECT
                newUIItem.worldObject = pickedObject;

                Transform holdPoint = (handIndex == 0) ? leftHoldPoint : rightHoldPoint;

                //pickedObject.transform.SetParent(holdPoint);
                NetworkIdentity identity = pickedObject.GetComponent<NetworkIdentity>();

                if (identity != null)
                {
                    CmdPickupObject(identity.netId, handIndex);
                }

                pickedObject.transform.localPosition = Vector3.zero;
                pickedObject.transform.localRotation = Quaternion.identity;

                Rigidbody rb = pickedObject.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;

                Collider col = pickedObject.GetComponent<Collider>();
                if (col != null) col.enabled = false;

                if (handIndex == 0)
                    leftHeldObject = pickedObject;
                else
                    rightHeldObject = pickedObject;

                Debug.Log(itemOnObject.name + " picked by hand index: " + handIndex);

                SetHandMeshState(handIndex, false);
            }
            else
            {
                Debug.LogWarning("Object " + hit.collider.name + " par 'Item' component nahi mila.");
            }
        }
    }


    [Command]
    void CmdPickupObject(uint objectNetId, int handIndex)
    {
        if (!NetworkServer.spawned.TryGetValue(objectNetId, out NetworkIdentity identity))
            return;

        GameObject obj = identity.gameObject;

        // Prevent double pickup
        if (obj.transform.parent != null)
            return;

        // Attach on server
        RpcAttachObject(objectNetId, netIdentity.netId, handIndex);
    }


    [ClientRpc]
    void RpcAttachObject(uint objectNetId, uint playerNetId, int handIndex)
    {
        if (!NetworkClient.spawned.TryGetValue(objectNetId, out NetworkIdentity objIdentity))
            return;

        if (!NetworkClient.spawned.TryGetValue(playerNetId, out NetworkIdentity playerIdentity))
            return;

        DualHooks hooks = playerIdentity.GetComponent<DualHooks>();
        GameObject obj = objIdentity.gameObject;

        Transform holdPoint = (handIndex == 0)
            ? hooks.leftHoldPoint
            : hooks.rightHoldPoint;

        obj.transform.SetParent(holdPoint);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = obj.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }



    public void SetHeldObjectState(int handIndex, bool state)
    {
        if (handIndex == 0 && leftHeldObject != null)
        {
            leftHeldObject.SetActive(state);
        }

        if (handIndex == 1 && rightHeldObject != null)
        {
            rightHeldObject.SetActive(state);
        }
    }

    public void SetHandMeshState(int handIndex, bool state)
    {
        if (handIndex == 0)
        {
            lft_hand_mesh.SetActive(state);
        }
        else
        {
            rt_hand_mesh.SetActive(state);
        }
    }



    [Command]
    private void CmdDestroyHeldObject(uint netId)
    {
        if (NetworkServer.spawned.TryGetValue(netId, out NetworkIdentity identity))
        {
            NetworkServer.Destroy(identity.gameObject);
        }
    }



    // [BONUS] Haath khaali karne ke liye
    public void DropItem(int handIndex)
    {
        GameObject heldObject = (handIndex == 0) ? leftHeldObject : rightHeldObject;

        if (heldObject != null)
        {
            NetworkIdentity identity = heldObject.GetComponent<NetworkIdentity>();

            if (identity != null)
            {
                CmdDestroyHeldObject(identity.netId);
                Debug.Log("Again  ......Testinggggg");

            }
            else
            {
                Destroy(heldObject);
                Debug.Log("Testinggggg");

            }

            if (handIndex == 0)
            {
                leftHeldObject = null;
                lft_hand_mesh.SetActive(true);
                Debug.Log("Mesh left hand (Slot 0)");

            }
            else
            {
                rightHeldObject = null;
                rt_hand_mesh.SetActive(true);
                Debug.Log("Mesh right hand (Slot 1)");


            }
        }

        if (handIndex == 0)
        {
            inventoryManager.RemoveSelectedLeftItem(true);

        }
        else
        {
            inventoryManager.RemoveSelectedRightItem(true);

        }
    }




    

}