using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour ,IDropHandler
{
    [Header("Hand Slot Settings")]
    public bool isHandSlot = false;
    public int handIndex = -1; // 0 = left, 1 = right

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0)
        {
            InventoryItem inventoryItem =
                eventData.pointerDrag.GetComponent<InventoryItem>();

            inventoryItem.parentAfterDrag = transform;

            // 🔥 If item came from hand
            if (inventoryItem.sourceHandIndex == 0)
            {
                Debug.Log("Item came from LEFT hand");
            }
            else if (inventoryItem.sourceHandIndex == 1)
            {
                Debug.Log("Item came from RIGHT hand");
            }
            else
            {
                Debug.Log("Item did not come from hand");
            }

         
        }
    }





}
