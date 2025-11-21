using UnityEngine;
using static InteractManager;

public class ShopItem : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private UIShopItem uIShopItem;

    private PlayerEquipment playerEquipment;
    public void Interact(InteractManager interactor, InteractManager.InteractAction action)
    {
        switch (action)
        {
            case InteractAction.EStart:
                uIShopItem.OnPressE();
                Buy();
                break;

            case InteractAction.R:
                Debug.Log($"Odebráno z košíku: {itemSO.ItemName}");
                break;

            case InteractAction.Hold:
                Debug.Log($"Nakupuješ víc kusù: {itemSO.ItemName}");
                break;

        }
    }

    public void OnHoverEnter(InteractManager interactor)
    {
          Debug.Log($"Hover over: {itemSO.ItemName}");
          if(playerEquipment == null )
            playerEquipment = interactor.GetPlayerEquipment();
         
          uIShopItem.SetShowCanvas(true, itemSO, playerEquipment.GetIconSourceByType(itemSO.itemType));


        //   UIManager.Instance.ShowTooltip($"{itemName} - {price} coinù");
    }

    public void OnHoverExit()
    {
            Debug.Log($"Hover exit: {itemSO.ItemName}");

        uIShopItem.SetShowCanvas(false, itemSO, playerEquipment.GetIconSourceByType(itemSO.itemType));

        //  UIManager.Instance.HideTooltip();
    }

    public void Buy()
    {
        if (MoneyManager.Instance.TrySpend(itemSO.price) && playerEquipment != null)
        {
            playerEquipment.UpgradeTool(itemSO);
            uIShopItem.SetShowCanvas(false, itemSO, playerEquipment.GetIconSourceByType(itemSO.itemType));

            Debug.Log("Bought!");
            // Give upgrade...
        }
        else
        {
            uIShopItem.SetnotEnoughMoney();
            Debug.Log("Not enough money!");
        }
    }
}
