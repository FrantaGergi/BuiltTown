using UnityEngine;

public class OpenStorageCommand : ICommand
{
    private Storage storage;
    private InventoryManager inventoryManager;

    public OpenStorageCommand(Storage storage, InventoryManager inventoryManager)
    {
        this.storage = storage;
        this.inventoryManager = inventoryManager;
    }
    
    public void Execute()
    {
        inventoryManager.OpenStorage(storage);
    }
}

