using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Mirror;
public class PlayerCam : NetworkBehaviour   
{
    [SyncVar]
    private float syncedYRotation;

    public float sensX;
    public float sensY;
    public float multiplier;

    public Transform orientation;
    public Transform camHolder;

    float xRotation;
    float yRotation;

    


    [Header("Fov")]
    public bool useFluentFov;
    public PlayerMovementDashing pm;
    public Rigidbody rb;
    public Camera cam;
    public float minMovementSpeed;
    public float maxMovementSpeed;
    public float minFov;
    public float maxFov;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            // 🔹 Mouse Input
            float mouseX = Input.GetAxisRaw("Mouse X") * sensX;
            float mouseY = Input.GetAxisRaw("Mouse Y") * sensY;

            yRotation += mouseX * multiplier;

            xRotation -= mouseY * multiplier;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            // 🔹 Rotate camera and body
            camHolder.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);

            // 🔹 Send rotation to server
            CmdSyncRotation(yRotation);

            if (useFluentFov)
                HandleFov();
        }
        else
        {
            // 🔹 Apply synced rotation for remote players
            orientation.rotation = Quaternion.Euler(0, syncedYRotation, 0);
        }
    }


    [Command]
    void CmdSyncRotation(float yRot)
    {
        syncedYRotation = yRot;
    }


    public override void OnStartLocalPlayer()
    {
        // ✅ SIRF local player ka camera ON
        cam.enabled = true;
        AudioListener listener = cam.GetComponent<AudioListener>();
        if (listener != null) listener.enabled = true;


    }

    public override void OnStartClient()
    {
        // ✅ NON-local players ka camera OFF
        if (!isLocalPlayer)
        {
            cam.enabled = false;
            AudioListener listener = cam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }


    private void HandleFov()
    {
        float moveSpeedDif = maxMovementSpeed - minMovementSpeed;
        float fovDif = maxFov - minFov;

        float rbFlatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;
        float currMoveSpeedOvershoot = rbFlatVel - minMovementSpeed;
        float currMoveSpeedProgress = currMoveSpeedOvershoot / moveSpeedDif;

        float fov = (currMoveSpeedProgress * fovDif) + minFov;

        float currFov = cam.fieldOfView;

        float lerpedFov = Mathf.Lerp(fov, currFov, Time.deltaTime * 200);

        cam.fieldOfView = lerpedFov;
    }

    public void DoFov(float endValue)
    {
        GetComponent<Camera>().DOFieldOfView(endValue, 0.25f);
    }

    public void DoTilt(float zTilt)
    {
        transform.DOLocalRotate(new Vector3(0, 0, zTilt), 0.25f);
    }



    //public override void OnStartLocalPlayer()
    //{
    //    cam.enabled = true;
    //}
    //public override void OnStartClient()
    //{
    //    if (!isLocalPlayer)
    //        cam.enabled = false;
    //}
}