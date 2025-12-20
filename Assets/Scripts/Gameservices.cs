using System.Resources;
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

    private bool T_Goal = false;
    private bool T_UpgradeTools = false;
    private bool T_MapOpened = false;
    private bool T_NPCSystem = false;
    private bool T_ShopSystem = false;
    private bool T_Interaction = false;

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;

        DontDestroyOnLoad(gameObject);
        if (Player == null)
            Player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Start()
    {
       // Invoke(nameof(SetTutorialGoal), 2f);
    }

    public void SetTutorialGoal()
    {
        if (T_Goal)
            return;

        Debug.Log("Start Tutorial");
        informationController.ShowText(
            "Welcome!",
            "Your goal is to mine resources, build houses and earn money.\r\nExpand your business and automate work with NPCs.",
            6f, true);

        Invoke(nameof(HowToDo), 7f);
    }

    private void HowToDo()
    {
        informationController.ShowText(
            "How to progress",
            "Mine resources → build houses → earn money → upgrade and hire NPCs.",
            6f, true);

        Invoke(nameof(HowWorksMapAndSystems), 7f);
    }

    private void HowWorksMapAndSystems()
    {
        informationController.ShowText(
            "Controls overview",
            "TAB – minimap\r\nQ – NPC management\r\nMouse wheel – switch tools",
            6f, true);

        Invoke(nameof(HowWorksTools), 7f);
    }

    private void HowWorksTools()
    {
        informationController.ShowText(
            "Tools",
            "Axe and pickaxe are used to mine different resources.\r\nUse the correct tool for the correct resource.",
            6f, true);

        T_Goal = true;
    }

    public void OnMapOpen()
    {
        if (T_MapOpened)
            return;

        informationController.ShowText(
                "Minimap",
    "<color=#00FF00>●</color> unlocked\n" +
    "<color=#FFA500>●</color> available\n" +
    "<color=#FF0000>●</color> locked",
            4f, true);

        T_MapOpened = true;
    }

    public void OnNPCManagerOpen()
    {
        if (T_NPCSystem)
            return;

        informationController.ShowText(
            "NPC Manager",
            "Hire NPCs who work for you.\r\nMore NPCs = faster progress.",
            4f, true);

        T_NPCSystem = true;
    }

    public void OnShopEntered()
    {
        if (T_ShopSystem)
            return;

        informationController.ShowText(
            "Shop",
            "Upgrade tools and hire NPCs here.\r\nInvestments boost your income.",
            5f, true);

        T_ShopSystem = true;
    }

    public void OnBuildCompleted()
    {
        if (T_Interaction)
            return;

        informationController.ShowText(
            "Interaction",
            "The (I) icon means the object can be opened.\r\nPress E.",
            4f, true);

        T_Interaction = true;
    }

    public void SetTutorialUpgradeTools()
    {
        if (T_UpgradeTools)
            return;
    }
}
