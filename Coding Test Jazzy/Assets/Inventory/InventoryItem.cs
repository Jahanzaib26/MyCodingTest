using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


public class InventoryItem : MonoBehaviour , IBeginDragHandler,IDragHandler,IEndDragHandler
{



    public GameObject worldObject;





    public int sourceHandIndex = -1;

    [Header("UI")]
    public Image image;
    public Text countText;

    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;
    public bool isProcessed = false;

    [HideInInspector]public Transform parentAfterDrag;


    //private void Start()
    //{
    //    InitilizeItem(item);
    //}


    public void InitilizeItem(Item newitem)
    {
        item = newitem;
        image.sprite = newitem.image;
        ResfreshCount();
    }

    public void ResfreshCount()
    {
        countText.text= count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        InventorySlot oldSlot = GetComponentInParent<InventorySlot>();



        if (oldSlot != null)
        {
            Debug.Log("Dragged from slot. isHandSlot = " + oldSlot.isHandSlot +
                      " handIndex = " + oldSlot.handIndex);
        }

        if (oldSlot != null && oldSlot.isHandSlot)
        {
            sourceHandIndex = oldSlot.handIndex;
            Debug.Log("Source hand set to: " + sourceHandIndex);
        }
        else
        {
            sourceHandIndex = -1;
        }




        if (oldSlot != null && oldSlot.isHandSlot)
        {
            sourceHandIndex = oldSlot.handIndex;
            Debug.Log("Drag started from hand index: " + sourceHandIndex);
        }
        else
        {
            sourceHandIndex = -1;
        }

        image.raycastTarget = false;
        parentAfterDrag = transform.parent;
        transform.SetParent(transform.root);
    }



    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;
        transform.SetParent(parentAfterDrag);

        InventorySlot newSlot = parentAfterDrag.GetComponent<InventorySlot>();

        // ---------------------------------------------------
        // 1) If item came FROM a hand and is dropped to MAIN slot
        // ---------------------------------------------------
        if ((newSlot == null || !newSlot.isHandSlot) && sourceHandIndex != -1)
        {
            // Hide world object FIRST (using correct hand reference)
            DualHooks.instance.SetHeldObjectState(sourceHandIndex, false);

            // Show that hand mesh again
            DualHooks.instance.SetHandMeshState(sourceHandIndex, true);

            // Now clear hand reference
            if (sourceHandIndex == 0) DualHooks.instance.leftHeldObject = null;
            if (sourceHandIndex == 1) DualHooks.instance.rightHeldObject = null;
        }

        // ---------------------------------------------------
        // 2) If item dropped INTO a hand slot (from main or other hand)
        // ---------------------------------------------------
        if (newSlot != null && newSlot.isHandSlot)
        {
            int newHand = newSlot.handIndex;

            // Assign this UI item's world object to that hand
            if (newHand == 0) DualHooks.instance.leftHeldObject = worldObject;
            if (newHand == 1) DualHooks.instance.rightHeldObject = worldObject;

            // Parent the world object to correct hold point
            if (worldObject != null)
            {
                Transform hold = (newHand == 0) ? DualHooks.instance.leftHoldPoint : DualHooks.instance.rightHoldPoint;
                worldObject.transform.SetParent(hold);
                worldObject.transform.localPosition = Vector3.zero;
                worldObject.transform.localRotation = Quaternion.identity;
                worldObject.SetActive(true);
            }

            // Hide that hand mesh because hand is holding item
            DualHooks.instance.SetHandMeshState(newHand, false);
        }

        // Reset
        sourceHandIndex = -1;
    }




}
