using Unity.IO.LowLevel.Unsafe;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public abstract class InventoryBase : MonoBehaviour
{
    [SerializeField]
    protected Transform slotsHolder;
    public InventorySlot[] slots;
    protected bool isOpen;
    public bool IsOpen => isOpen;

    protected virtual void Awake()
    {
        slots = new InventorySlot[slotsHolder.childCount];
        slots = slotsHolder.GetComponentsInChildren<InventorySlot>(true);
    }

    public InventorySlot FindEmptySlot()
    {
        InventorySlot emptySlot = null;

        for (int i = 0; i < slots.Length; i++)
        {


            if (slots[i].HeldItem == null)
            {
                emptySlot = slots[i];
                break;
            }
        }

        return emptySlot;
    }

    public void ClearTextInSlots()
    {

        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].ClearText();
        }


    }


    public bool ItemPicked(GameObject pickedObject, GameObject itemPrefab)
    {
        InventorySlot targetSlot = null;
        PickableItem pickableItem = pickedObject.GetComponent<PickableItem>();

        for (int i = 0; i < slots.Length; i++) // checking if some stack is available
        {
            if (slots[i].HeldItem != null)
            {
                InventoryItem inventoryItem = slots[i].HeldItem;

                if (inventoryItem != null && pickableItem != null &&
                inventoryItem.itemScriptableObject == pickableItem.itemScriptableObject &&
                inventoryItem.stackCurrent < inventoryItem.stackMax)
                {
                    targetSlot = slots[i];
                    break;
                }
            }
        }

        if (targetSlot == null) // so if stack is not available; go for empty slot
        {
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i].HeldItem == null)
                {
                    targetSlot = slots[i];
                    break;
                }
            }
        }

        if (targetSlot != null)
        {

            InventoryItem inventoryItem = targetSlot.HeldItem != null
            ? targetSlot.HeldItem : null;

            if (inventoryItem != null)
            {
                inventoryItem.stackCurrent++;
                inventoryItem.SetCurrentStackText(targetSlot.TextMeshProUGUI);
            }
            else
            {
                GameObject newItem = Instantiate(itemPrefab);
                inventoryItem = newItem.GetComponent<InventoryItem>();
                inventoryItem.itemScriptableObject = pickableItem.itemScriptableObject;
                inventoryItem.stackCurrent = 1;////
                
                targetSlot.SetHeldItem(inventoryItem);
            }

            Destroy(pickedObject);

            return true;
        }

        return false;

    }
}
