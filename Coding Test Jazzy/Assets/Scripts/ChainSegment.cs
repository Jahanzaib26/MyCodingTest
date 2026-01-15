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

    public void SetNumber(int num)
    {
        number = num;

        // Destroy old mesh if exists
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

    public void SetFollowOffset(int offset)
    {
        positionIndexOffset = offset;
    }

    public bool stopFollow = false; // default false

    void Update()
    {
        if (stopFollow || playerHistory == null || playerHistory.positions.Count == 0)
            return;

        // Clamp index so we never go out of range
        int clampedIndex = Mathf.Min(positionIndexOffset, playerHistory.positions.Count - 1);
        Vector3 targetPos = playerHistory.positions[clampedIndex];

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }



}
