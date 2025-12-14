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
            this.uIBuildingMailboxController.SetOption(
                this,
                FormatNumber(buildingReward.FinalRentAmount),
                FormatNumber(buildingReward.FinalRewardAmount));
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
            buildingReward.FinalRentAmount, buildingReward.timeToEarn, buildingReward.currentTimeToEarn);

        mailboxArrow.HideArrow();
        buildingReward.Trigger();

    }

    internal void SellBuilding()
    {

        if (uIBuildingMailboxController == null) return;

        buildingReward.mode = BuildingReward.RewardMode.OneTime;
        uIBuildingMailboxController.SetInformation(
            buildingReward.FinalRewardAmount, 0, 0);

        mailboxArrow.HideArrow();
        buildingReward.Trigger();
    }



    private string FormatNumber(long value)
    {
        if (value < 1000)
            return value.ToString();

        if (value < 1_000_000)
        {
            double v = Math.Floor((value / 1000d) * 10) / 10; // floor na 0.1k
            return v.ToString("0.#") + "k";
        }

        if (value < 1_000_000_000)
        {
            double v = Math.Floor((value / 1_000_000d) * 10) / 10; // floor na 0.1M
            return v.ToString("0.#") + "M";
        }

        {
            double v = Math.Floor((value / 1_000_000_000d) * 10) / 10; // floor na 0.1B
            return v.ToString("0.#") + "B";
        }
    }


}
