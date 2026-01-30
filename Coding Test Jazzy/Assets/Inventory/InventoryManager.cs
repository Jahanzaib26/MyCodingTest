using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private int maxitemCount = 4;

    public InventorySlot[] slots;

    public GameObject inventoryItemPrefab;
    public GameObject inventoryItemCanvas;

    int selectedLeftSlot = -1;




    public void OpenInventory(bool value)
    {
        inventoryItemCanvas.SetActive(value);
    }



    public bool AddItem(Item item)
    {


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
            return itemInSlot.item;

        }
        return null;

    }


    public Item RemoveSelectedLeftItem(bool use)
    {
        InventorySlot slot = slots[0];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            return itemInSlot.item;

            if (use)
            {
                itemInSlot.count--;

                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
            }
        }
        return null;

    }



    public Item GetSelectedRightItem()
    {
        InventorySlot slot = slots[1];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            return itemInSlot.item;           
        }
        return null;

    }


    public Item RemoveSelectedRightItem(bool use)
    {
        InventorySlot slot = slots[1];
        InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

        if (itemInSlot != null)
        {
            return itemInSlot.item;

            if (use)
            {
                itemInSlot.count--;

                if (itemInSlot.count <= 0)
                {
                    Destroy(itemInSlot.gameObject);
                }
            }
        }
        return null;

    }
}
