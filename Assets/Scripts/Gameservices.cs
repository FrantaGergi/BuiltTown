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

    private bool T_Goal = false;
    private bool T_UpgradeTools = false;
    private bool T_MapOpened = false;
    private bool T_NPCSystem = false;
    private bool T_ShopSystem = false;
    private bool T_Interaction = false;

    //public NPCManager npcManager;

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
        Invoke(nameof(SetTutorialGoal), 2f);
    }
    public void SetTutorialGoal()
    {
        if (T_Goal)
            return;

        Debug.Log("Start Tutorial");
        informationController.ShowText("Vítej!",
            "Tvým cílem je těžit suroviny, stavět domy a vydělávat peníze.\r\nRozšiřuj svůj business a automatizuj práci pomocí NPC.",
            6f, true);

        Invoke(nameof(HowToDo), 7f);
        
    }
    private void HowToDo()
    {
        informationController.ShowText("Jak postupovat",
           "Těž suroviny → stav domy → vydělávej peníze → vylepšuj a najímej NPC.",
           6f, true);

        Invoke(nameof(HowWorksMapAndSystems), 7f);
    }
    private void HowWorksMapAndSystems()
    {
        informationController.ShowText("Přehled ovládání",
           "TAB – minimapa\r\nQ – správa NPC\r\nKolečko myši – přepínání nástrojů",
           6f, true);
        Invoke(nameof(HowWorksTools), 7f);
    }

    private void HowWorksTools()
    {
        informationController.ShowText("Nástroje",
           "Sekera a krumpáč slouží k těžbě různých surovin.\r\nPoužij správný nástroj na správný resource.",
           6f, true);

        T_Goal = true;
    }

    public void OnMapOpen()
    {
        if(T_MapOpened)
            return;

        informationController.ShowText("Minimapa",
   "\U0001f7e2 odemčeno\r\n\U0001f7e0 dostupné\r\n🔴 zamčeno",
   4f, true);
        T_MapOpened = true;

    }
    public void OnNPCManagerOpen()
    {
        if(T_NPCSystem)
            return;

        informationController.ShowText("NPC manažer",
  "Najímej NPC, kteří pracují za tebe.\r\nVíce NPC = rychlejší postup.",
  4f, true);

        T_NPCSystem = true;
    }

    public void OnShopEntered()
    {
        if (T_ShopSystem)
            return;
        informationController.ShowText("Obchod",
  "Zde vylepšuješ nástroje a najímáš NPC.\r\nInvestice zrychlí tvůj výdělek.",
  5f, true);
    
        T_ShopSystem = true;
    }

    public void OnBuildCompleted()
    {
        if (T_Interaction)
            return;

        informationController.ShowText("Interakce",
   "Ikona (I) znamená, že objekt můžeš otevřít.\r\nStiskni E.",
  4f, true); 

        T_Interaction = true;

    }



    public void SetTutorialUpgradeTools()
    {
        if (T_UpgradeTools)
            return;
    }


}
