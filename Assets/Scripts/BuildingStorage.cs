using UnityEngine;

public class BuildingStorage : MonoBehaviour
{
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] ItemType itemType = ItemType.Wood;

    [SerializeField] private CircularClickGame CircularClickGame;
    [SerializeField] private BuildingCanvV2 buildingCanv;


    [Header("References ->"),SerializeField] Building building;


    private void Start()
    {
        if(building != null)
            UpdateprogressBarAndText();
    }

    public void Setup(Building building)
    {
        this.building = building;
        UpdateprogressBarAndText();
    }
    //E
    public void OnInteract(InteractManager interactor, bool isStarting)
    {
        if (inventoryManager == null)
            inventoryManager = interactor.GetInventoryManager();

        int inventoryCount = inventoryManager.GetResourceCount(itemType);
        int countToAdd = Mathf.Min(building.HowManyMissing(itemType), inventoryCount);


        if (inventoryCount < 1 || countToAdd < 1)
            return;

        if (isStarting)
            StartClickGame();
        else
            StopClickGame();


    }


    public void StartClickGame()
    {
        if (CircularClickGame != null)
            CircularClickGame.StartGame(this);
    }
    public void StopClickGame()
    {
        if (CircularClickGame != null)
            CircularClickGame.StopGame();
    }
    public void TryHitInGame()
    {
       CircularClickGame.TryHit();
    }
    public void AddSource(int multiplier)
    {
        int inventoryCount = inventoryManager.GetResourceCount(itemType);


        if (building.GetCurrentCost(itemType) >= building.GetCost(itemType) || inventoryCount == 0)
        {
            StopClickGame();
            return;
        }
        
        ItemSO itemSO = inventoryManager.GetItemSOByItemType(itemType);

        int countToAdd = Mathf.Min(building.HowManyMissing(itemType), inventoryCount);


        int count = 1 * multiplier;

        if(countToAdd < count)
            count = countToAdd;



        building.AddResource(count, itemType);
        inventoryManager.RemoveResourceFromHotbar(itemSO, count);

        UpdateprogressBarAndText();

        if (building.GetCurrentCost(itemType) >= building.GetCost(itemType) || inventoryManager.GetResourceCount(itemType) == 0)
        {
            StopClickGame();
            return;
        }
    }

    public void UpdateprogressBarAndText()
    {
        if(itemType == ItemType.Wood)
        {
            buildingCanv.SetProgressBarSmooth(
          Mathf.Clamp01((float)building.curentWoodCost / (float)building.woodCost));
            buildingCanv.SetText(building.curentWoodCost, building.woodCost);
        } 
        else if (itemType == ItemType.Stone)
        {
            buildingCanv.SetProgressBarSmooth(
          Mathf.Clamp01((float)building.curentStoneCost / (float)building.stoneCost));
            buildingCanv.SetText(building.curentStoneCost, building.stoneCost);
        }
        else if (itemType == ItemType.Ore)
        {
            buildingCanv.SetProgressBarSmooth(
          Mathf.Clamp01((float)building.curentOreCost / (float)building.oreCost));
            buildingCanv.SetText(building.curentOreCost, building.oreCost);
        }
            

       
    }


    public void OnHoverEnter(InteractManager interactor)
    {
        if (inventoryManager == null)
            inventoryManager = interactor.GetInventoryManager();




    }

}
