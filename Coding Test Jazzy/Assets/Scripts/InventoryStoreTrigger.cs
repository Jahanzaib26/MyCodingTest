using UnityEngine;
using Mirror;

public class InventoryStoreTrigger : NetworkBehaviour
{
    private InventoryManager localInventory;

    void OnTriggerEnter(Collider other)
    {
        InventoryManager inv = other.GetComponentInChildren<InventoryManager>();

        if (inv == null) return;
        if (!inv.isLocalPlayer) return;

        Debug.Log("🏪 Press E to store inventory items");
        localInventory = inv;
    }

    void OnTriggerExit(Collider other)
    {
        if (localInventory == null) return;

        InventoryManager inv = other.GetComponentInChildren<InventoryManager>();
        if (inv != localInventory) return;

        Debug.Log("❌ Left store area");
        localInventory = null;
    }

    void Update()
    {
        if (localInventory == null) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("🟡 E pressed → store inventory");
            localInventory.CmdStoreInventoryItems();
            localInventory = null;
        }
    }
}
