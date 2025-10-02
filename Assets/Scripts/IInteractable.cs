

using System.Windows.Input;
using UnityEngine;

public interface IInteractable
{
    void Interact(InteractManager interactor, InteractManager.InteractAction action);

    // jen vizuální efekty
    void OnHoverEnter(InteractManager interactor);
    void OnHoverExit();

}