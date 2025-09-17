

using System.Windows.Input;

public interface IInteractable
{

    string GetInteractionDescription();
    ICommand GetInteractionCommand(InventoryManager inventoryManager);

}