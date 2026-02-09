using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class TotalCollectManager : NetworkBehaviour
{
    public static TotalCollectManager Instance;

    [SyncVar(hook = nameof(OnTotalChanged))]
    public int totalCollect = 10000; // start value (example)

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        // jab client join kare, current value se UI sync
        UpdateAllPlayerUI(totalCollect);
    }


    void OnTotalChanged(int oldValue, int newValue)
    {
        UpdateAllPlayerUI(newValue);
    }

    void UpdateAllPlayerUI(int value)
    {
        foreach (InventoryManager inv in FindObjectsOfType<InventoryManager>())
        {
            inv.UpdateTotalCollectUI(value);
        }
    }

    [Server]
    public void Add(int amount)
    {
        totalCollect += amount;
    }

    [Server]
    public void Remove(int amount)
    {
        totalCollect -= amount;
        if (totalCollect < 0)
            totalCollect = 0;
    }
}
