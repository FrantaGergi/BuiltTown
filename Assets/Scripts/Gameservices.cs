using UnityEngine;

public class GameServices : MonoBehaviour
{
    public static GameServices I { get; private set; }

    public Transform Player;
    public ResourceMapManager resourceMapManager;
    public InventoryManager playerInventory;
    public NPCManager NPCManager;
    public UIBuildingMailboxController uiBuildingMailboxController;
    public InformationController informationController;
    public AudioSource audioSource;

    private bool tutorialMoveShown = false;
    private bool tutorialMapShown = false;
    private bool tutorialBuildingSelectShown = false;
    private bool tutorialMiningShown = false;
    private bool tutorialToolSwitchShown = false;
    private bool tutorialItemDeliveredShown = false;
    private bool tutorialInteractionShown = false;
    private bool tutorialNPCManagerShown = false;
    private bool tutorialShopOnMapShown = false;

    private bool T_ShopSystem = false;
    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player").transform;

        ShowMoveTutorial();
    }



    public void OnShopEntered() {
        if (T_ShopSystem) return;
        informationController.ShowText(
            "Shop",
            "Upgrade tools and hire NPCs here.\r\nInvestments boost your income.",
            5f, true);
        T_ShopSystem = true; 
    }

    // === Tutorial Methods ===

    public void ShowMoveTutorial()
    {
        if (tutorialMoveShown) return;
        informationController.ShowText(
            "Movement",
            "Use WASD or arrow keys to move around.",
            4f, true);
        tutorialMoveShown = true;
    }

    public void ShowMapTutorial()
    {
        if (tutorialMapShown) return;
        informationController.ShowText(
            "Open Map",
            "Press TAB to open the map.",
            4f, true);
        tutorialMapShown = true;
    }

    public void ShowBuildingSelectionTutorial()
    {
        if (tutorialBuildingSelectShown) return;
        informationController.ShowText(
            "Select Building",
            "Click on the green icon to choose a building to construct.",
            4f, true);
        tutorialBuildingSelectShown = true;
    }

    public void ShowMiningTutorial()
    {
        if (tutorialMiningShown) return;
        informationController.ShowText(
            "Mine Resources",
            "Close map\n Go to a resource node to mine.",
            6f, true);
        tutorialMiningShown = true;
    }

    public void ShowToolSwitchTutorial()
    {
        if (tutorialToolSwitchShown) return;
        informationController.ShowText(
            "Switch Tools",
            "Use mouse scroll to select the correct tool for the resource.",
            4f, true);
        tutorialToolSwitchShown = true;
    }

    public void ShowItemDeliveryTutorial()
    {
        if (tutorialItemDeliveredShown) return;
        informationController.ShowText(
            "Deliver Resources",
            "Take the mined item to the building holder and place it there.",
            4f, true);
        tutorialItemDeliveredShown = true;
    }

    public void ShowInteractionTutorial()
    {
        if (tutorialInteractionShown) return;
        informationController.ShowText(
            "Interaction",
            "The (I) icon means the object can be opened. Press E to interact.",
            6f, true);
        tutorialInteractionShown = true;
    }

    public void ShowNPCManagerTutorial()
    {
        if (tutorialNPCManagerShown) return;
        informationController.ShowText(
            "NPC Manager",
            "Press Q to open the NPC manager and manage your hired NPCs.",
            4f, true);
        tutorialNPCManagerShown = true;
    }
    public void ShowShopOnMapTutorial()
    {
        if (tutorialShopOnMapShown) return;
        informationController.ShowText(
             "Shop and Districts",
             "The shop is on the map. Orange districts on the minimap can be bought.",
        5f, false);
        tutorialShopOnMapShown = true;
    }
}
