using Mirror;
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


    int selectedLeftSlot = -1;



    private HashSet<Item> addedItems = new HashSet<Item>();
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
        InventorySlot slot = slots[0];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            // ✅ player price add
            AddItemPrice(itemInSlot.item);

            // ✅ global total collect minus
            if (TotalCollectManager.Instance != null)
            {
                CmdRemoveFromTotal(itemInSlot.item.price);
            }

            return itemInSlot.item;
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
            RemoveItemPrice(itemInSlot.item);
            return itemInSlot.item;
        }


        return null;

    }

    void AddItemPrice(Item item)
    {
        if (item == null) return;

        // agar ye item pehle hi add ho chuka hai
        if (addedItems.Contains(item))
            return;


        addedItems.Add(item);
        totalPrice += item.price;
        priceText.text =totalPrice + "$";
    }

    void RemoveItemPrice(Item item)
    {
        if (item == null) return;

        // sirf tab minus karo jab pehle add hua ho
        if (!addedItems.Contains(item))
            return;

        addedItems.Remove(item);
        totalPrice -= item.price;

        // safety
        if (totalPrice < 0)
            totalPrice = 0;

        priceText.text = totalPrice + "$";
    }



    public Item GetSelectedRightItem()
    {
        InventorySlot slot = slots[1];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            // ✅ player price add
            AddItemPrice(itemInSlot.item);

            // ✅ global total collect minus
            if (TotalCollectManager.Instance != null)
            {
                CmdRemoveFromTotal(itemInSlot.item.price);
            }

            return itemInSlot.item;
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
            RemoveItemPrice(itemInSlot.item);
            return itemInSlot.item;
        }


        return null;

    }

    //[Command]
    //public void CmdStoreInventoryItems()
    //{
    //    int totalDroppedPrice = 0;

    //    for (int i = 0; i < slots.Length; i++)
    //    {
    //        InventoryItem itemInSlot = slots[i].GetComponentInChildren<InventoryItem>();

    //        if (itemInSlot == null) continue;

    //        Item item = itemInSlot.item;

    //        if (item == null) continue;
    //        if (item.price <= 0) continue;

    //        // 🔹 total price collect
    //        totalDroppedPrice += item.price;

    //        // 🔹 world drop
    //        if (item.itemPrefab != null)
    //        {
    //            Vector3 dropPos = transform.position + transform.forward * 2f;
    //            GameObject dropped = Instantiate(item.itemPrefab, dropPos, Quaternion.identity);
    //            NetworkServer.Spawn(dropped);
    //        }

    //        // 🔹 inventory UI clean
    //        NetworkServer.Destroy(itemInSlot.gameObject);
    //    }

    //    // 🔴 inventory empty check
    //    if (totalDroppedPrice == 0)
    //    {
    //        TargetInventoryEmpty(connectionToClient);
    //        return;
    //    }

    //    // 🔴 GLOBAL TOTAL COLLECT minus
    //    if (TotalCollectManager.Instance != null)
    //    {
    //        TotalCollectManager.Instance.Remove(totalDroppedPrice);
    //    }
    //}

    //[TargetRpc]
    //void TargetInventoryEmpty(NetworkConnection target)
    //{
    //    Debug.Log("⚠️ Inventory is empty");
    //}




}
