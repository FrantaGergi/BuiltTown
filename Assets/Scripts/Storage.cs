using System.Collections;
using System.Collections.Generic;
using System.Windows.Input;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Storage : MonoBehaviour, IInteractable
{
    public string name;
    public int size;
    public bool isOpen = false;
    [Header("MAX is 42, dont forget!!!")]
    public List<StorageItem> items = new List<StorageItem>();
   

   
    private void Start()
    {
        int itemsToAdd = size - items.Count;
        for(int i = 0; i < itemsToAdd; i++)
        {
            items.Add(new StorageItem(0, null));
        }
    }

   

    public string GetInteractionDescription()
    {
        return   $"Otevøít {name}";
    }

    public ICommand GetInteractionCommand(InventoryManager inventoryManager)
    {
      //  isOpen = !inventoryManager.storageParent.IsOpen; // ! bcs isopen will be the 2. option
        return new OpenStorageCommand(this, inventoryManager);
    }

    
}
