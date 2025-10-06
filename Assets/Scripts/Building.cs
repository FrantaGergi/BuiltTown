using UnityEngine;

public class Building : MonoBehaviour
{
    [Header("Costs")]
    [Range(0,10000000),SerializeField]
    public int woodCost;
    [Range(0, 10000000), SerializeField]
    public int stoneCost;
    [Range(0, 10000000), SerializeField]
    public int oreCost;

    private int curentWoodCost;
    private int curentStoneCost;
    private int curentOreCost;

    [Header("SO")]
    public BuildingSO buildingSO;
    public BuildingCanvController buildingCanvController;

    private InventoryManager inventoryManager;

    private bool use3Resources = true;


    private void Start()
    {
        // Optional: Initialize or validate costs here
        if(woodCost <= 0 || stoneCost <= 0)
        {
            Debug.LogError("Costs cannot be negative!");
        }
        if(oreCost <= 0)
        {
           use3Resources = false;
           buildingCanvController.HideThirdSource();
        }
        if(buildingCanvController == null)
        {
            buildingCanvController = GetComponent<BuildingCanvController>();
        }

        curentWoodCost = 0;
        curentStoneCost = 0;
        curentOreCost = 0;
    }
   
    public int HowManyWoodMissing()
    {
        return Mathf.Max(0, woodCost - curentWoodCost);
    }
    public int HowManyStoneMissing()
    {
        return Mathf.Max(0, stoneCost - curentStoneCost);
    }
    public int HowManyMetalMissing()
    {
        if (use3Resources)
        {
            return Mathf.Max(0, oreCost - curentOreCost);
        }
        else
        {
            return 0;
        }
    }
    public void OnInteract(InteractManager interactor)
    {
        if (inventoryManager == null)
            inventoryManager = interactor.GetInventoryManager();

        int woddC = inventoryManager.GetResourceCount(ItemType.Wood);
        int stoneC = inventoryManager.GetResourceCount(ItemType.Stone);
        int oreC = inventoryManager.GetResourceCount(ItemType.Ore);

        int woodToAdd = Mathf.Min(HowManyWoodMissing(), woddC);
        int stoneToAdd = Mathf.Min(HowManyStoneMissing(), stoneC);
        int oreToAdd = Mathf.Min(HowManyMetalMissing(), oreC);

        if (woodToAdd > 0)
        {
            ItemSO itemSO = inventoryManager.GetItemSOByItemType(ItemType.Wood);
            if (itemSO != null)
            {
                int a = AddResource(itemSO, woodToAdd);
                if(a != 0)
                    Debug.LogError("zbytek se nepøidal wood");

                inventoryManager.RemoveResourceFromHotbar(itemSO,woodToAdd);
            }
           
        }
        if (stoneToAdd > 0)
        {
            ItemSO itemSO = inventoryManager.GetItemSOByItemType(ItemType.Stone);
            if (itemSO != null)
            {
                int a = AddResource(itemSO, stoneToAdd);
                if(a != 0)
                    Debug.LogError("zbytek se nepøidal stone");

                inventoryManager.RemoveResourceFromHotbar(itemSO,stoneToAdd);
            }
           
        }
        if (oreToAdd > 0)
        {
            ItemSO itemSO = inventoryManager.GetItemSOByItemType(ItemType.Ore);
            if (itemSO != null)
            {
                int a = AddResource(itemSO, oreToAdd);
                if(a != 0)
                    Debug.LogError("zbytek se nepøidal ore");

                inventoryManager.RemoveResourceFromHotbar(itemSO,oreToAdd);
            }
           
        }

    }
    public void OnHoverEnter(InteractManager interactor)
    {
        if (inventoryManager == null)
            inventoryManager = interactor.GetInventoryManager();


        var types = inventoryManager.GetAllItemTypesInHotbar();

        buildingCanvController.SetAllButtonsGray();

        foreach (var item in types)
        {
            buildingCanvController.SetButtonSprite(item);
        }

        buildingCanvController.SetProgressBar(
               ItemType.Wood, Mathf.Clamp01((float)curentWoodCost / (float)woodCost));
        buildingCanvController.SetProgressBar(
               ItemType.Stone, Mathf.Clamp01((float)curentStoneCost / (float)stoneCost));
        buildingCanvController.SetProgressBar(
               ItemType.Ore, Mathf.Clamp01((float)curentOreCost / (float)oreCost));

        buildingCanvController.SetText(ItemType.Wood, curentWoodCost, woodCost);
        buildingCanvController.SetText(ItemType.Stone, curentStoneCost, stoneCost);
        buildingCanvController.SetText(ItemType.Ore, curentOreCost, oreCost);

    }
    // Returns the amount of resource that couldn't be added (if any)
    public int AddResource(ItemSO itemSO, int count)
    {
        int rest = 0;
        if (itemSO.itemType == ItemType.Wood)
        {
            curentWoodCost += count;
            buildingCanvController.PressButton(ItemType.Wood);
            buildingCanvController.SetProgressBarSmooth(
                ItemType.Wood, Mathf.Clamp01((float)curentWoodCost / (float)woodCost));
            buildingCanvController.SetText(ItemType.Wood, curentWoodCost, woodCost);

            if (curentWoodCost >= woodCost)
            {
                rest =  curentWoodCost - woodCost;
                curentWoodCost = woodCost;
                buildingCanvController.SetButtonToFinished(ItemType.Wood);
            }
        }
        else if(itemSO.itemType == ItemType.Stone)
        {
            curentStoneCost += count;
            buildingCanvController.PressButton(ItemType.Stone);
            buildingCanvController.SetProgressBarSmooth(
                ItemType.Stone, Mathf.Clamp01((float)curentStoneCost / (float)stoneCost));
            buildingCanvController.SetText(ItemType.Stone, curentStoneCost, stoneCost);

            if (curentStoneCost >= stoneCost)
            {
                rest = curentStoneCost - stoneCost;
                curentStoneCost = stoneCost;
                buildingCanvController.SetButtonToFinished(ItemType.Stone);
            }
        }
        else if(itemSO.itemType == ItemType.Ore && use3Resources)
        {
            curentOreCost += count;
            buildingCanvController.PressButton(ItemType.Ore);
            buildingCanvController.SetProgressBarSmooth(
                ItemType.Ore, Mathf.Clamp01((float)curentOreCost / (float)oreCost));
            buildingCanvController.SetText(ItemType.Ore, curentOreCost, oreCost);

            if (curentOreCost >= oreCost)
            {
                rest = curentOreCost - oreCost;
                curentOreCost = oreCost;
                buildingCanvController.SetButtonToFinished(ItemType.Ore);
            }
        }
        return rest;
    }
   

}
