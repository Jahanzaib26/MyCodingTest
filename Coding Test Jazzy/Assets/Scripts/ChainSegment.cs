using UnityEngine;

public class ChainSegment : MonoBehaviour
{
    [HideInInspector] public PlayerPositionHistory playerHistory;
    public GameObject[] numberPrefabs; // 0-9 meshes
    public float followSpeed = 10f;

    private GameObject currentNumberObj;
    private int number;

    // For following player path
    private int positionIndexOffset = 0;

    // Stop movement when game over
    public bool stopFollow = false;

    /// <summary>
    /// Sets the number this segment should display
    /// </summary>
    /// <param name="num"></param>
    public void SetNumber(int num)
    {
        number = num;

        // Destroy old number mesh if exists
        if (currentNumberObj != null)
            Destroy(currentNumberObj);

        // Instantiate new number mesh
        if (numberPrefabs != null && number >= 0 && number < numberPrefabs.Length)
        {
            currentNumberObj = Instantiate(numberPrefabs[number], transform);
            currentNumberObj.transform.localPosition = Vector3.zero;
            currentNumberObj.transform.localRotation = Quaternion.Euler(0, 180f, 0); // fix rotation
        }
    }

    /// <summary>
    /// Sets how far behind the head this segment follows
    /// </summary>
    /// <param name="offset"></param>
    public void SetFollowOffset(int offset)
    {
        positionIndexOffset = offset;
    }

    void Update()
    {
        // Stop movement immediately if level failed
        if (stopFollow || playerHistory == null || playerHistory.positions.Count == 0)
            return;

        // Clamp index so we never go out of bounds
        int clampedIndex = Mathf.Min(positionIndexOffset, playerHistory.positions.Count - 1);
        Vector3 targetPos = playerHistory.positions[clampedIndex];

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
