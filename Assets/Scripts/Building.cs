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

    public int curentWoodCost { get; private set; }
    public int curentStoneCost { get; private set; }
    public int curentOreCost { get; private set; }


    public BuildingProgress buildingProgress;

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
        }
     

        curentWoodCost = 0;
        curentStoneCost = 0;
        curentOreCost = 0;

        SetBuildingProgressMaterials();

        buildingProgress.OnConstructionCompleted += BuildingCompleted;
    }

    public void AddResource(int count, ItemType itemType)
    {
        if(itemType == ItemType.Wood)
        {
            curentWoodCost += count;
            if(curentWoodCost > woodCost)
            {
                curentWoodCost = woodCost;
            }
        }
        else if(itemType == ItemType.Stone)
        {
            curentStoneCost += count;
            if (curentStoneCost > stoneCost)
            {
                curentStoneCost = stoneCost;
            }
        }
        else if(itemType == ItemType.Ore && use3Resources)
        {
            curentOreCost += count;
            if (curentOreCost > oreCost)
            {
                curentOreCost = oreCost;
            }
        }
        SetBuildingProgressMaterials();
    }

    public int HowManyMissing(ItemType itemType)
    {
        if(itemType == ItemType.Wood)
        {
            return Mathf.Max(0, woodCost - curentWoodCost);
        }
        else if(itemType == ItemType.Stone)
        {
            return Mathf.Max(0, stoneCost - curentStoneCost);
        }
        else if(itemType == ItemType.Ore)
        {
            return Mathf.Max(0, oreCost - curentOreCost);
        }
        else
        {
            Debug.LogWarning($"HowManyMissing: Neznámý itemType {itemType}");
            return 0;
        }
    }


    private void SetBuildingProgressMaterials()
    {
        buildingProgress.wood.required = woodCost;
        buildingProgress.stone.required = stoneCost;
        buildingProgress.ore.required = oreCost;
        buildingProgress.wood.current = curentWoodCost;
        buildingProgress.stone.current = curentStoneCost;
        buildingProgress.ore.current = curentOreCost;
        buildingProgress.UpdateVisuals();

    }
    public int GetCurrentCost(ItemType itemType)
    {
        if (itemType == ItemType.Wood)
            return curentWoodCost;
        else if (itemType == ItemType.Stone)
            return curentStoneCost;
        else if (itemType == ItemType.Ore)
            return curentOreCost;
        else
        {
            Debug.LogWarning($"GetCurrentCost: Neznámý itemType {itemType}");
            return 0;
        }
    }
    public int GetCost(ItemType itemType)
    {
        if (itemType == ItemType.Wood)
            return woodCost;
        else if (itemType == ItemType.Stone)
            return stoneCost;
        else if (itemType == ItemType.Ore)
            return oreCost;
        else
        {
            Debug.LogWarning($"GetCurrentCost: Neznámý itemType {itemType}");
            return 0;
        }
    }


    private void BuildingCompleted()
    {
        MoneyManager.Instance.AddMoney(152500); // Example reward
    }


}
