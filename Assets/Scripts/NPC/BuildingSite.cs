using UnityEngine;

public class BuildingSite : MonoBehaviour, IBuildingSite
{
    [SerializeField] public Building buildingCore;
    [SerializeField] public ResourceHolder resourceHolder;

    public int ID = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     if(buildingCore == null)
        {
           buildingCore = GetComponent<Building>();
            if(buildingCore == null)
                Debug.LogError("BuildingSite: No District component found on the GameObject.");
        }   
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // chceme i vedet zda je potreba nosit do holderu nejakou surovinu pro stavbu
    public bool NeedsResourceForCollectors(ItemType type)
    {
        if (buildingCore != null)
        {
            return (buildingCore.HowManyMissing(type) - resourceHolder.GetResourceCount(type)) > 0;
        }
        return false;
    }

    public bool NeedsResourceForBuilders(ItemType type)
    {
        if (buildingCore != null)
        {
            return buildingCore.HowManyMissing(type) > 0;
        }
        return false;
    }
    public void AddResourceByCollector(ItemType type, int amount)
    {
        if (resourceHolder != null)
        {
            resourceHolder.AddResource(type, amount );
        }
    }
    public void AddResourceByBuilder(ItemType type, int amount)
    {
        if (buildingCore != null)
        {
            buildingCore.AddResource(amount, type);
        }
    }

    public Vector3 GetHolderPosition()
    {
        return resourceHolder != null ? resourceHolder.transform.position : transform.position;
    }
}
