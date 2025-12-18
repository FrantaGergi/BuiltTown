using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static InteractManager;

public class ShopItem : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private ItemSO itemSO;
    [SerializeField] private UIShopItem uIShopItem;

    private PlayerEquipment playerEquipment;

    public void Interact(InteractManager interactor, InteractManager.InteractAction action)
    {


        if (playerEquipment.pi != null && playerEquipment.pi.currentActionMap != null 
            && playerEquipment.pi.currentActionMap.name == "UI")
            return;

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
          if(playerEquipment == null )
            playerEquipment = interactor.GetPlayerEquipment();
         
            if( playerEquipment == null )
            Debug.LogError("PlayerEquipment is null in ShopItem OnHoverEnter");

        if(uIShopItem == null )
            Debug.LogError("UIShopItem is null in ShopItem OnHoverEnter");
        uIShopItem.SetShowCanvas(true, itemSO, playerEquipment.GetIconSourceByType(itemSO.itemType));


        //   UIManager.Instance.ShowTooltip($"{itemName} - {price} coinù");
    }

    public void OnHoverExit()
    {
            uIShopItem.SetShowCanvas(false, itemSO, playerEquipment.GetIconSourceByType(itemSO.itemType));

        //  UIManager.Instance.HideTooltip();
    }

    public void Buy()
    {
        if (MoneyManager.Instance.TrySpend(itemSO.price) && playerEquipment != null)
        {
            if(itemSO.itemType == ItemType.None)
            {
                uIShopItem.SetShowCanvas(false, itemSO, itemSO.icon);
                NPCBuyed();
            }
            else
            {
                playerEquipment.UpgradeTool(itemSO);
                uIShopItem.SetShowCanvas(false, itemSO, playerEquipment.GetIconSourceByType(itemSO.itemType));

            }


            // Give upgrade...
        }
        else
        {
            uIShopItem.SetnotEnoughMoney();
        }
    }

    private void NPCBuyed()
    {
        var npcObj = Instantiate(itemSO.prefab, transform.position, Quaternion.identity);
        var baseNPC = npcObj.GetComponent<BaseNPC>();

        NPCManager npcManager = GameServices.I.NPCManager;

        AchievementService.OnNPCHired();

        string displayDescript = "Valim si to na zachode, chapes";

        if (npcObj.TryGetComponent<MinerRole>(out var minerRole))
        {
            npcManager.RegisterNPC(baseNPC, UINPC.Role.Miner, GetRandomFullName(), displayDescript);
            return;
        }
        if (npcObj.TryGetComponent<CollectorRole>(out var collectorRole))
        {
            npcManager.RegisterNPC(baseNPC, UINPC.Role.Collector, GetRandomFullName(), displayDescript);
            return;
        }
        if (npcObj.TryGetComponent<BuilderRole>(out var builderRole))
        {
            npcManager.RegisterNPC(baseNPC, UINPC.Role.Builder, GetRandomFullName(), displayDescript);
            return;
        }

    }


    private static readonly List<string> firstNames = new List<string>
    {
        "Jan", "Petr", "Tomas", "Lukas", "Adam",
        "David", "Martin", "Jakub", "Filip", "Ondrej"
    };

    private static readonly List<string> lastNames = new List<string>
    {
        "Novak", "Svoboda", "Dvorak", "Prochazka", "Kucera",
        "Benes", "Horak", "Jelinek", "Kral", "Ruzicka"
    };

    /// <summary>
    /// Vrátí náhodné jméno + pøíjmení, oddìlené pomocí \n (pro UI).
    /// </summary>
    public static string GetRandomFullName()
    {
        string first = firstNames[Random.Range(0, firstNames.Count)];
        string last = lastNames[Random.Range(0, lastNames.Count)];

        return $"{first}\n{last}";
    }

}
