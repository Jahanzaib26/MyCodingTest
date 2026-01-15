using System.Collections.Generic;
using UnityEngine;

public class ChainManager : MonoBehaviour
{
    public PlayerValueController player;
    public PlayerPositionHistory playerHistory;
    public GameObject chainSegmentPrefab;

    public float segmentSpacing = 5;
    private List<ChainSegment> segments = new List<ChainSegment>();

    void Start()
    {
        InitializeSnake();
    }

    public void InitializeSnake()
    {
        if (player == null || playerHistory == null || chainSegmentPrefab == null)
        {
            Debug.LogError("ChainManager references missing!");
            return;
        }

        UpdateChain();
    }

    public void UpdateChain()
    {
        int headValue = player.currentValue;

        while (segments.Count > headValue - 1)
        {
            RemoveSegment();
        }

        for (int i = segments.Count; i < headValue - 1; i++)
        {
            AddSegment(headValue - 1 - i); // countdown order
        }

        for (int i = 0; i < segments.Count; i++)
        {
            segments[i].SetFollowOffset((i + 1) * Mathf.RoundToInt(segmentSpacing));
        }
    }

    void AddSegment(int number)
    {
        if (chainSegmentPrefab == null)
        {
            Debug.LogError("ChainSegment prefab missing!");
            return;
        }

        GameObject segObj = Instantiate(chainSegmentPrefab, transform.position, transform.rotation);
        ChainSegment seg = segObj.GetComponent<ChainSegment>();

        if (seg == null)
        {
            Debug.LogError("ChainSegment script missing on prefab!");
            return;
        }

        seg.playerHistory = playerHistory;
        seg.SetNumber(number);
        segments.Add(seg);

        Debug.Log("Added segment with number: " + number);
    }

    void RemoveSegment()
    {
        if (segments.Count == 0) return;

        ChainSegment seg = segments[segments.Count - 1];
        segments.RemoveAt(segments.Count - 1);
        if (seg != null)
            Destroy(seg.gameObject);
    }
}
