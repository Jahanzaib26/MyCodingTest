using UnityEngine;

public class PlayerHeadMovement : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 8f;
    public float horizontalSpeed = 12f;

    [Header("Horizontal Limits")]
    public float minX = -2.5f;
    public float maxX = 2.5f;

    private float targetX;
    private Vector2 lastInputPos;
    private bool isDragging;

    void Start()
    {
        targetX = transform.position.x;
    }

    void Update()
    {
        MoveForward();
        HandleInput();
        MoveHorizontal();
    }

    void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime);
    }

    void HandleInput()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput();
#else
        HandleTouchInput();
#endif
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            lastInputPos = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }

        if (!isDragging) return;

        Vector2 currentPos = Input.mousePosition;
        float deltaX = (currentPos.x - lastInputPos.x) / Screen.width;

        targetX += deltaX * horizontalSpeed;
        targetX = Mathf.Clamp(targetX, minX, maxX);

        lastInputPos = currentPos;
    }

    void HandleTouchInput()
    {
        if (Input.touchCount == 0) return;

        Touch touch = Input.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            lastInputPos = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            float deltaX = (touch.position.x - lastInputPos.x) / Screen.width;

            targetX += deltaX * horizontalSpeed;
            targetX = Mathf.Clamp(targetX, minX, maxX);

            lastInputPos = touch.position;
        }
    }

    void MoveHorizontal()
    {
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * 10f);
        transform.position = pos;
    }
}
