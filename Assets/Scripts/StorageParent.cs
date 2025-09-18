using UnityEngine;

public class StorageParent : MonoBehaviour
{
 
    public Storage lastStorage;

    private  void Awake()
    {
    }
    /*
    public void Load(Storage storage, GameObject itemPrefab)
    {
        for (int i = 0; i < slots.Length; i++) // all nonactive slots
        {
            slots[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < storage.size; i++)  // some turn on slots
        {
            slots[i].gameObject.SetActive(true);
        }


        for (int i = 0; i < slots.Length; i++) // delete existing items from other load
        {
    
            if (slots[i].HeldItem != null) // if is there item -> deleteLikeObject
                slots[i].DeleteItem();
        }

        int index = 0;

        foreach (StorageItem storageItem in storage.items) // set items
        {
            if (storageItem.itemScriptableObject != null) // if the slot has item(SO) so we wanna create a put into slot
            {
                GameObject newItem = Instantiate(itemPrefab);
                InventoryItem item = newItem.GetComponent<InventoryItem>();
                item.itemScriptableObject = storageItem.itemScriptableObject;
                item.stackCurrent = storageItem.currentStack;


                slots[index].SetHeldItem(item);

            }
            index++;
        }




        ClearTextInSlots();
     
    }


    public void Save(Storage storage)
    {
    

        storage.items.Clear();

        for (int i = 0; i < slots.Length; i++)
        {
 ;
            if (slots[i].gameObject.activeSelf && slots[i].HeldItem != null)
            {
                InventoryItem inventoryItem = slots[i].HeldItem;

                storage.items.Add(new StorageItem(inventoryItem.stackCurrent, inventoryItem.itemScriptableObject));

            }
            else
            {
                storage.items.Add(new StorageItem(0, null));

            }
        }
    }

    public void Open(Storage storage, GameObject itemPrefab)
    {
        lastStorage = storage;
        isOpen = true;
        gameObject.SetActive(true);
        Load(storage, itemPrefab);


    }
    public void Close(Storage storage)
    {
        lastStorage = null;
        isOpen = false;

        gameObject.SetActive(false);
        Save(storage);

    }

    */

}
