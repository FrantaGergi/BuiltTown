using System;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    InventoryItem draggedObject;
    InventorySlot lastItemSlot;

    //[SerializeField]
    //public InventoryParent InventoryParent;
    [SerializeField]
    public StorageParent storageParent;
    [SerializeField]
    private Camera Cam;

    [Header("fireTarget-- parent of all point of holding items --> guns, bandage..")]
    [SerializeField] Transform handParent;

    [SerializeField]
    HotbarParent hotbarParent;


    //ClickTimer clickTimer = new ClickTimer(); // double click register for better

    [SerializeField]
    public GameObject itemPrefab;

    public bool IsLocked { get; private set; } = false; // if is locked, player cant interact with inventory, but can open storage


    public void LockInventory(bool lockInv)
    {
        IsLocked = lockInv;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        HotbarItemChanged();
    }

    // Update is called once per frame
    void Update()
    {



        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (IsLocked) // if is locked, we can open storage, but not inventory
            {
                return;
            }


            if (storageParent.lastStorage != null)    // closing storage press tab
            {
                CloseStorage(storageParent.lastStorage);
            }
        }
        if (draggedObject != null)
        {
            draggedObject.transform.position = Input.mousePosition;

        }
        CheckForHotbarInput();


    }
    public void OpenStorage(Storage storage)
    {
        storageParent.Open(storage, itemPrefab);
    }
    public void CloseStorage(Storage storage)
    {
        storageParent.Close(storage);

    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftShift)) // split
        {
            SplitItems(eventData, true); // fifty
        }
        else if (eventData.button == PointerEventData.InputButton.Left && Input.GetKey(KeyCode.LeftControl)) // one and other
        {
            SplitItems(eventData, false); // one

        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();
            if (slot != null && slot.HeldItem != null)
            {

                draggedObject = slot.HeldItem;
                slot.SetHeldItem(null);
                lastItemSlot = slot;
                draggedObject.transform.SetParent(lastItemSlot.transform.parent.parent);// priority on visible of all UI

               // if (clickTimer.RegisterClick()) // double click --> set place for item --> tum --> tum
               //     ChangeSlotItemByAvailable(); // this make moving if is open storage, or only inventory, on this base it is comunicated

                HotbarItemChanged();

            }
        }
    }

    private void SplitItems(PointerEventData eventData, bool fiftyfifty)
    {
        GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;

        if (clickedObject.TryGetComponent<InventorySlot>(out InventorySlot slot) && slot.HeldItem != null)
        {
            InventoryItem item = slot.HeldItem;
            if (item.stackCurrent <= 1)
                return;

            InventorySlot emptySlot = null;
            GameObject parent = clickedObject.transform.parent.gameObject;
            if (parent.transform.parent.gameObject == storageParent.gameObject) // storage
            {
                emptySlot = storageParent.FindEmptySlot();

            }
            else if (parent == hotbarParent.gameObject) // hotbar
            {
                emptySlot = hotbarParent.FindEmptySlot();

            }
          

            if (emptySlot != null) // ternary operator - testing -
            {
                int firstPart = fiftyfifty ? (item.stackCurrent / 2) : 0;
                int secondPart = fiftyfifty ? (item.stackCurrent - firstPart) : 0;

                GameObject newItem = Instantiate(itemPrefab);
                InventoryItem inventoryItem = newItem.GetComponent<InventoryItem>();
                inventoryItem.itemScriptableObject = slot.HeldItem.itemScriptableObject;
                inventoryItem.stackCurrent = fiftyfifty ? firstPart : 1;////
                emptySlot.SetHeldItem(inventoryItem);
                slot.HeldItem.stackCurrent = fiftyfifty ? secondPart : (slot.HeldItem.stackCurrent - 1);
                slot.HeldItem.SetCurrentStackText(slot.TextMeshProUGUI);
            }

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {

        if (draggedObject != null && eventData.pointerCurrentRaycast.gameObject != null && eventData.button == PointerEventData.InputButton.Left) // left click
        {
            GameObject clickedObject = eventData.pointerCurrentRaycast.gameObject;
            InventorySlot slot = clickedObject.GetComponent<InventorySlot>();

            if (slot != null && slot.HeldItem == null)
            {
                slot.SetHeldItem(draggedObject);
            }
            else if (slot != null && slot.HeldItem != null && slot.HeldItem.stackCurrent ==
                slot.HeldItem.stackMax
                || (slot != null && slot.HeldItem != null && slot.HeldItem.itemScriptableObject !=
                draggedObject.itemScriptableObject)) //switch also if stack is max or if items are not same
            {
                lastItemSlot.SetHeldItem(slot.HeldItem);
                slot.SetHeldItem(draggedObject);
            }
            //<--- switch

            //--> fill stack
            else if (slot != null && slot.HeldItem != null && slot.HeldItem.stackCurrent < slot.HeldItem.stackMax
                && (slot != null && slot.HeldItem.itemScriptableObject == draggedObject.itemScriptableObject))         // fill stack
            {


                FillStack(slot);

            }
            else if (clickedObject.name != "DropItemPlace")
            {
                lastItemSlot.SetHeldItem(draggedObject);
            }
            else
            {
                DropAllDraggedItems();
            }


            HotbarItemChanged();
            draggedObject = null;

        }
    }

    private void DropAllDraggedItems()
    {
        Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
        Vector3 position = ray.GetPoint(3);
        position = new Vector3(position.x, Cam.transform.parent.position.y, position.z); // what if the guy had camera directly down, drop should be failure

        for (int i = 0; i < draggedObject.stackCurrent; i++) // what if fireTarget has more than one stack?? :D
        {
            GameObject newItem = Instantiate(draggedObject.itemScriptableObject.prefab, position, new Quaternion());
            newItem.GetComponent<PickableItem>().itemScriptableObject = draggedObject.itemScriptableObject;
        }

        lastItemSlot.SetHeldItem(null);
        Destroy(draggedObject.gameObject);
    }

    private void FillStack(InventorySlot slot) // always the calling this have to be in onkey down, else it would be problem, the dragged obj could be null
    {
        InventoryItem slotHeldItem = slot.HeldItem;


        int itemsToFillStack = slotHeldItem.stackMax - slotHeldItem.stackCurrent; // count available to fill

        if (itemsToFillStack >= draggedObject.stackCurrent)
        {
            slotHeldItem.stackCurrent += draggedObject.stackCurrent;
            slotHeldItem.SetCurrentStackText(slot.TextMeshProUGUI);
            Destroy(draggedObject.gameObject);
            draggedObject = null;
        }
        else
        {

            slotHeldItem.stackCurrent += itemsToFillStack;
            draggedObject.stackCurrent -= itemsToFillStack;
            slotHeldItem.SetCurrentStackText(slot.TextMeshProUGUI);

            lastItemSlot.SetHeldItem(draggedObject);
            draggedObject = null;
        }

    }


    private bool FillItemToStackIfIsAvailable(InventorySlot[] slots)
    {


        for (int i = 0; i < slots.Length; i++)
        {

            if (slots[i] != null && slots[i].HeldItem != null && draggedObject != null
                && slots[i].HeldItem.stackCurrent < slots[i].HeldItem.stackMax
               && (slots[i].HeldItem.itemScriptableObject == draggedObject.itemScriptableObject)
               )         // fill stack
            {

                FillStack(slots[i]);
                return true;

            }
        }

        return false;

    }




    private void ChangeSlotItemByAvailable() // this has IF,sssss, its for that when fireTarget want selected item place on other place than is current
    {
        InventorySlot emptySlot = null;
        if (lastItemSlot.transform.parent.parent.gameObject == storageParent.gameObject) // storage
        {

            // to inventory
         //   if (FillItemToStackIfIsAvailable(InventoryParent.slots)) // this mean, that if is it possible += stack, if not, well keep going 
            //    return;

                if (FillItemToStackIfIsAvailable(hotbarParent.slots))
                    return;

                emptySlot = hotbarParent.FindEmptySlot();

            

        }
        else // hortbar
        {
            if (storageParent.IsOpen) // to storage
            {
                if (FillItemToStackIfIsAvailable(storageParent.slots))
                    return;

                emptySlot = storageParent.FindEmptySlot();
            }
        }

        // now we would have final slot for implementation
        if (emptySlot != null) // if is avaible
        {
            emptySlot.SetHeldItem(draggedObject);
            draggedObject = null;
        }
    }

    internal void ItemPicked(GameObject pickedObject) // take and set to inv or hotbar, and then w check if we can filltostack (also we cant forget HeldItem set like null)
    { // ONLY FOR ONE ITEM, IF PLAYER COULD TAKE MORE THAN ITEM IN ONE TIME, THERE WILL BE PROBLEM
        bool isPicked = false;


        isPicked = hotbarParent.ItemPicked(pickedObject, itemPrefab); // first of all we prefer when we collect item to hotbar not inventory :P

        if (!isPicked)      // if is the hotbar full, lets get started in inventory
        {
      //      isPicked = InventoryParent.ItemPicked(pickedObject, itemPrefab);
        }

        if (isPicked)
            HotbarItemChanged();
    }

    private void CheckForHotbarInput()
    {

        if (hotbarParent.IsHotbarChange())
            HotbarItemChanged();


    }
    private void HotbarItemChanged()
    {

        for (int i = 0; i < handParent.childCount; i++)
        {
            handParent.GetChild(i).gameObject.SetActive(false);
        }


        //the same instruction == turn off any active slot
        if (hotbarParent.selectedHotbarSlot == -1)
        {
            hotbarParent.slots[hotbarParent.lastSelectedhotbarSlot].transform.GetChild(1).gameObject.SetActive(false);

            if (hotbarParent.slots[hotbarParent.lastSelectedhotbarSlot].HeldItem != null) // hand viewer
            {
                for (int i = 0; i < handParent.childCount; i++)
                {
                    if (handParent.GetChild(i).GetComponent<ItemHand>().itemScriptableObject // current itemSlot is equal to item in hand to turn off
                       == hotbarParent.slots[hotbarParent.lastSelectedhotbarSlot].
                       HeldItem.itemScriptableObject
                      )
                    {
                        handParent.GetChild(i).gameObject.SetActive(false); // turn off
                        break;
                    }
                }
            }
            return;
        }

        foreach (InventorySlot slot in hotbarParent.slots)
        {

            if (slot == hotbarParent.slots[hotbarParent.selectedHotbarSlot])
            {

                slot.gameObject.transform.GetChild(1).gameObject.SetActive(true);

                if (slot.HeldItem != null) // hand viewer
                {
                    for (int i = 0; i < handParent.childCount; i++)
                    {

                        if (handParent.GetChild(i).GetComponent<ItemHand>().itemScriptableObject // current itemSlot is equal to item in hand
                           == hotbarParent.slots[hotbarParent.selectedHotbarSlot].
                           HeldItem.itemScriptableObject
                          )
                        {
                            handParent.GetChild(i).gameObject.SetActive(true);
                            break;
                        }


                    }
                }
            }
            else
            {
                slot.transform.GetChild(1).gameObject.SetActive(false);

                //nonAcitve
            }
        }
    }

    public ItemSO GetSelectedItemFromHotbar()
    {
        if (hotbarParent.selectedHotbarSlot == -1)
        {
            return null; // no item selected
        }
        if (hotbarParent.slots[hotbarParent.selectedHotbarSlot].HeldItem == null)
        {

            return null; // no item selected
        }
        ItemSO item = hotbarParent.slots[hotbarParent.selectedHotbarSlot].HeldItem.itemScriptableObject;

        return item;
    }
    public bool TrydeleteItemFromHotbar(ItemSO item)
    {
        foreach (InventorySlot slot in hotbarParent.slots)
        {
            if (slot.HeldItem != null && slot.HeldItem.itemScriptableObject == item)
            {
                slot.DeleteItem();
                HotbarItemChanged();
                return true;
            }
        }
        return false;

    }
}
