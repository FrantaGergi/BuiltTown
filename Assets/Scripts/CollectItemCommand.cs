using UnityEngine;

public class CollectItemCommand :ICommand
{
    private GameObject item;
    private InventoryManager inventoryManager;


    public CollectItemCommand(GameObject item, InventoryManager inventoryManager)
    {
        this.item = item;
        this.inventoryManager = inventoryManager;
    }

    public void Execute()
    {
        inventoryManager.ItemPicked(item);
     
    }
}
