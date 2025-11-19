using System;
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
  

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnOpenMailbox(UIBuildingMailboxController uIBuildingMailboxController)
    {
        if(this.uIBuildingMailboxController == null)
            this.uIBuildingMailboxController = uIBuildingMailboxController;

        // Check the mode of the building reward
   

        if (buildingReward.mode == BuildingReward.RewardMode.UnSetted)
            this.uIBuildingMailboxController.SetOption(this);
        else
        {
            this.uIBuildingMailboxController.SetInformation(
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

    internal void RentBuilding()
    {

        if (uIBuildingMailboxController == null)
        {
            Debug.LogWarning("UIBuildingMailboxController is null");
            return;
        }

        buildingReward.mode = BuildingReward.RewardMode.Recurring;
        uIBuildingMailboxController.SetInformation(
            buildingReward.ammountToShow, buildingReward.timeToEarn, buildingReward.currentTimeToEarn);

        mailboxArrow.HideArrow();
        buildingReward.Trigger();

    }

    internal void SellBuilding()
    {

        if (uIBuildingMailboxController == null) return;

        buildingReward.mode = BuildingReward.RewardMode.OneTime;
        uIBuildingMailboxController.SetInformation(
            buildingReward.ammountToShow, buildingReward.timeToEarn, buildingReward.currentTimeToEarn);

        mailboxArrow.HideArrow();
        buildingReward.Trigger();
    }

   
}
