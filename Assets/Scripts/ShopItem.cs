using UnityEngine;
using static InteractManager;

public class ShopItem : MonoBehaviour, IInteractable
{
    public string itemName;
    public int price;

    public void Interact(InteractManager interactor, InteractManager.InteractAction action)
    {
        switch (action)
        {
            case InteractAction.EStart:
                Debug.Log($"Do košíku: {itemName}");
                break;

            case InteractAction.R:
                Debug.Log($"Odebráno z košíku: {itemName}");
                break;

            case InteractAction.Hold:
                Debug.Log($"Nakupuješ víc kusù: {itemName}");
                break;

        }
    }

    public void OnHoverEnter(InteractManager interactor)
    {
          Debug.Log($"Hover over: {itemName}");
        //   UIManager.Instance.ShowTooltip($"{itemName} - {price} coinù");
    }

    public void OnHoverExit()
    {
            Debug.Log($"Hover exit: {itemName}");
        //  UIManager.Instance.HideTooltip();
    }
}
