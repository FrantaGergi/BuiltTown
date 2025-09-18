using UnityEngine;

public class PickableItem : MonoBehaviour, IInteractable
{
    public ItemSO itemScriptableObject;

    public ICommand GetInteractionCommand(InventoryManager inventoryManager)
    {
        return new CollectItemCommand(itemScriptableObject, inventoryManager);
    }

    public string GetInteractionDescription()
    {
        return "Sebrat";
    }
}
