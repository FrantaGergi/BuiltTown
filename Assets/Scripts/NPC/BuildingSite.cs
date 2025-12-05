using UnityEngine;

public class BuildingSite : MonoBehaviour, IBuildingSite
{
    [SerializeField] public Building buildingCore;

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

    public bool NeedsResource(ItemType type)
    {
        if (buildingCore != null)
        {
            return buildingCore.HowManyMissing(type) > 0;
        }
        return false;
    }

    public void AddResource(ItemType type, int amount)
    {
        if (buildingCore != null)
        {
            buildingCore.AddResource(amount, type);
        }
    }
}
