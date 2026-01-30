using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoTestInventory : MonoBehaviour
{

    public InventoryManager manager;
    public Item[] itemsToPickup;
   public void PickupIttem(int no)
    {
      bool result=  manager.AddItem(itemsToPickup[no]);

        if (result)
        {
            print("item Added");
        }
        else
        {
            print("item not added");

        }
    }
}
