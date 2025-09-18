using UnityEngine;

public class CollectItemCommand : ICommand
{
    private ItemSO itemSO;
    private InventoryManager inventoryManager;

    public CollectItemCommand(ItemSO itemSO, InventoryManager inventoryManager)
    {
        this.itemSO = itemSO;
        this.inventoryManager = inventoryManager;
    }

    public void Execute()
    {
        inventoryManager.AddResourceToHotbar(itemSO, 1);
        // pøípadnì znièit objekt ve svìtì
    }
}
