

using System.Windows.Input;
using UnityEngine;

public interface IInteractable
{
    void Interact(GameObject interactor, InteractManager.InteractAction action);

    // jen vizuální efekty
    void OnHoverEnter();
    void OnHoverExit();

}