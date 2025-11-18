using UnityEngine;

public class Mailbox : MonoBehaviour
{
    [SerializeField, Header("References")]
    private UIMailbox uiMailbox;
    [SerializeField]
    private MailboxArrow mailboxArrow;
    [SerializeField]
    private BuildingReward buildingReward;

    private UIBuildingMailboxController uIBuildingMailboxController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
     mailboxArrow.ShowArrow();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnOpenMailbox(UIBuildingMailboxController uIBuildingMailboxController)
    {
        if(uIBuildingMailboxController == null)
            this.uIBuildingMailboxController = uIBuildingMailboxController;

        if (buildingReward.mode == BuildingReward.RewardMode.UnSetted)
            uIBuildingMailboxController.SetOption();
        else
        {
            uIBuildingMailboxController.SetInformation(
    buildingReward.ammountToShow, buildingReward.timeToEarn, buildingReward.currentTimeToEarn);
            mailboxArrow.HideArrow();
        }

    
    }
    public void OnCloseMailbox()
    {
        uIBuildingMailboxController?.OnCloseMailbox();
        if(buildingReward.mode != BuildingReward.RewardMode.UnSetted)
        {
            mailboxArrow.HideArrow();
        }
    }
}
