using UnityEngine;
using static InteractManager;

/// <summary>
/// Umožòuje interakci hráèe s ResourceHolderem.
/// </summary>
public class ResourceHolderInteract : MonoBehaviour, IInteractable
{
    [SerializeField] private ResourceHolder holder;
    [SerializeField] private GameObject highlightObject;

    private InventoryManager playerInventory;

    void Start()
    {
        if (holder == null)
            holder = GetComponent<ResourceHolder>();

        holder.ResourceHolderUI.HideUI();
    }

    public void Interact(InteractManager interactor, InteractAction action)
    {
        if (playerInventory == null)
            playerInventory = interactor.GetInventoryManager();

        switch (action)
        {
            case InteractAction.EStart:
                // Vložit zdroje z inventáøe do holderu
                DepositResources();
                break;

            case InteractAction.R:
                // Vybrat všechny zdroje z holderu
                WithdrawAllResources();
                break;
        }
    }

    public void OnHoverEnter(InteractManager interactor)
    {
        if (playerInventory == null)
            playerInventory = interactor.GetInventoryManager();

        if (highlightObject != null)
            highlightObject.SetActive(true);

        holder.ResourceHolderUI.ShowUI();
    }

    public void OnHoverExit()
    {
        if (highlightObject != null)
            highlightObject.SetActive(false);

        holder.ResourceHolderUI.HideUI();
    }

    private void DepositResources()
    {
        if (playerInventory == null || holder == null) return;

        // Zkus vložit všechny typy zdrojù z hotbaru
        var types = playerInventory.GetAllItemTypesInHotbar();

        foreach (var type in types)
        {
            if (type == ItemType.Wood || type == ItemType.Stone || type == ItemType.Ore)
            {
                int count = playerInventory.GetResourceCount(type);
                if (count > 0)
                {
                    int added = holder.AddResource(type, count);
                    if (added > 0)
                    {
                        ItemSO itemSO = playerInventory.GetItemSOByItemType(type);
                        playerInventory.RemoveResourceFromHotbar(itemSO, added);
                    }
                }
            }
        }
    }

    private void WithdrawAllResources()
    {
        if (playerInventory == null || holder == null) return;

        var allResources = holder.RemoveAllResources();

        foreach (var kvp in allResources)
        {
            ItemSO itemSO = GameServices.I.resourceMapManager.GetResourceSO(kvp.Key);
            if (itemSO != null)
            {
                playerInventory.AddResourceToHotbar(itemSO, kvp.Value);
            }
        }
    }
}
