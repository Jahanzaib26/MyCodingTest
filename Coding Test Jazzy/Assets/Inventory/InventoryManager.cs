using Mirror;
using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class InventoryManager : NetworkBehaviour
{
    private int maxitemCount = 4;

    public InventorySlot[] slots;

    public GameObject inventoryItemPrefab;
    public GameObject inventoryItemCanvas;
    public Text totalCollectText;
    
    
    // ye track karega ke kaun se items ki quota already minus ho chuki hai
    //private HashSet<Item> quotaDeductedItems = new HashSet<Item>();



    int selectedLeftSlot = -1;



    //private HashSet<Item> addedItems = new HashSet<Item>();
    public Text priceText;
    private int totalPrice = 0;


    public void OpenInventory(bool value)
    {
        inventoryItemCanvas.SetActive(value);
    }
    public bool AddItem(Item item)
    {
        if (!isLocalPlayer) return false;

        if (TotalCollectManager.Instance != null)
        {
            CmdRemoveFromTotal(item.price);
        }

        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot != null && itemInSlot.item==item && itemInSlot.count< maxitemCount && item.stackable==true)
            {
                itemInSlot.count++;
                itemInSlot.ResfreshCount();
                return true;
            }
        }
        for (int i = 0; i < slots.Length; i++)
        {
            InventorySlot slot = slots[i];
            InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

            if (itemInSlot == null)
            {
                SpwanItem(item, slot);
                return true;
            }


        }
        return false;
    }

    [Command]
    void CmdRemoveFromTotal(int price)
    {
        TotalCollectManager.Instance.Remove(price);
    }

   


    public void UpdateTotalCollectUI(int value)
    {
        totalCollectText.text = value + "$";
    }



    public void SpwanItem(Item item,InventorySlot slot)
    {

        GameObject newItemGo = Instantiate(inventoryItemPrefab, slot.transform);

        InventoryItem inventoryItem = newItemGo.GetComponent<InventoryItem>();
        inventoryItem.InitilizeItem(item);

    }


    public Item GetSelectedLeftItem()
    {
        InventoryItem itemInSlot = slots[0].GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            // ❌ agar pehle process ho chuka hai → kuch mat karo
            if (itemInSlot.isProcessed)
                return itemInSlot.item;

            Item item = itemInSlot.item;

            // ✅ PEHLI DAFA CLICK
            AddItemPrice(item);

            if (TotalCollectManager.Instance != null)
            {
                CmdRemoveFromTotal(item.price);
            }

            // 🔒 lock this slot-item
            itemInSlot.isProcessed = true;

            return item;
        }

        return null;
    }





    public Item RemoveSelectedLeftItem(bool use)
    {
        InventorySlot slot = slots[0];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        //if (itemInSlot != null)
        //{
        //    return itemInSlot.item;

        //    if (use)
        //    {
        //        itemInSlot.count--;

        //        if (itemInSlot.count <= 0)
        //        {
        //            Destroy(itemInSlot.gameObject);
        //        }
        //    }
        //}

        if (itemInSlot != null)
        {
            // 🔓 unlock for next pickup
            itemInSlot.isProcessed = false;
            RemoveItemPrice(itemInSlot.item);
            return itemInSlot.item;
        }


        return null;

    }

    void AddItemPrice(Item item)
    {
        if (item == null) return;

        // agar ye item pehle hi add ho chuka hai
        //if (addedItems.Contains(item))
        //    return;


        //addedItems.Add(item);
        totalPrice += item.price;
        priceText.text =totalPrice + "$";
    }

    void RemoveItemPrice(Item item)
    {
        if (item == null) return;

        // sirf tab minus karo jab pehle add hua ho
        //if (!addedItems.Contains(item))
        //    return;

        //addedItems.Remove(item);
        totalPrice -= item.price;

        // safety
        if (totalPrice < 0)
            totalPrice = 0;

        priceText.text = totalPrice + "$";
    }



    public Item GetSelectedRightItem()
    {
        InventoryItem itemInSlot = slots[1].GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            if (itemInSlot.isProcessed)
                return itemInSlot.item;

            Item item = itemInSlot.item;

            AddItemPrice(item);

            if (TotalCollectManager.Instance != null)
            {
                CmdRemoveFromTotal(item.price);
            }

            itemInSlot.isProcessed = true;

            return item;
        }

        return null;
    }





    public Item RemoveSelectedRightItem(bool use)
    {
        InventorySlot slot = slots[1];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        //if (itemInSlot != null)
        //{
        //    return itemInSlot.item;

        //    //if (use)
        //    //{
        //    //    itemInSlot.count--;

        //    //    if (itemInSlot.count <= 0)
        //    //    {
        //    //        Destroy(itemInSlot.gameObject);
        //    //    }
        //    //}
        //}

        if (itemInSlot != null)
        {
            // 🔓 unlock for next pickup
            itemInSlot.isProcessed = false;
            RemoveItemPrice(itemInSlot.item);
            return itemInSlot.item;
        }


        return null;

    }

    [Server]
    public void ServerClearInventoryOnDeath(NetworkConnectionToClient targetConn)
    {
        Debug.Log("inventory reset");
        int refundAmount = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            InventoryItem itemInSlot = slots[i].GetComponentInChildren<InventoryItem>();
            if (itemInSlot == null) continue;

            Item item = itemInSlot.item;
            if (item == null) continue;

            refundAmount += item.price;
        }

        // 🔼 GLOBAL QUOTA refund
        if (refundAmount > 0 && TotalCollectManager.Instance != null)
        {
            TotalCollectManager.Instance.Add(refundAmount);
        }

        // 🧹 CLIENT inventory wipe
        TargetClearInventoryOnDeath(targetConn);
    }


    [TargetRpc]
    void TargetClearInventoryOnDeath(NetworkConnection target)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            InventoryItem itemInSlot = slots[i].GetComponentInChildren<InventoryItem>();
            if (itemInSlot != null)
                Destroy(itemInSlot.gameObject);
        }

        totalPrice = 0;
        priceText.text = "0$";

        Debug.Log("🧹 Inventory cleared on death");
    }

    public void BackToMainMenu()
    {
        if (!isLocalPlayer) return;

        Debug.Log("⬅️ Back button pressed");

        // 🔹 Steam lobby leave
        if (LobbyController.Instance != null &&
            LobbyController.Instance.CurrentLobbyID != 0)
        {
            SteamMatchmaking.LeaveLobby(
                new CSteamID(LobbyController.Instance.CurrentLobbyID)
            );

            LobbyController.Instance.CurrentLobbyID = 0;
            Debug.Log("🚪 Left Steam Lobby");
        }

        // 🔹 HOST → sab players bahar
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            Debug.Log("🛑 Host leaving → StopHost()");
            NetworkManager.singleton.StopHost();
        }
        // 🔹 CLIENT → sirf ye player bahar
        else if (NetworkClient.isConnected)
        {
            Debug.Log("🛑 Client leaving → StopClient()");
            NetworkManager.singleton.StopClient();
        }

        // 🔹 Cursor unlock
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        // ❌ SceneManager.LoadScene() NOT needed
        // Mirror khud Offline Scene load karega
    }


    






}
