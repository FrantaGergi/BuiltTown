using System;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{

    public string GetInteractionDescription()
    {
        return "Jedu jen z interactableojvectu";
    }

    public ICommand GetInteractionCommand(InventoryManager inventoryManager) // this is bad bro dont forget
    {
        throw new NullReferenceException(); // its better what if i forget :D
    }

}
