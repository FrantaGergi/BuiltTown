using UnityEngine;
using UnityEngine.InputSystem;

public class ChooserOfBuildingManager : MonoBehaviour
{
    public bool isChooserOfBuildingOpen = false;

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private UIChooserOfBuilding uiChooserOfBuilding;


    private Plot currentPlot;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiChooserOfBuilding.Hide();
        isChooserOfBuildingOpen = false;

    }

    public void CloseUIChooser()
    {
        uiChooserOfBuilding.Hide();
        isChooserOfBuildingOpen = false;

    }

    public void OpenBuildingChooser(Plot plot)
    {
        uiChooserOfBuilding.ShowBigHouseCosts(plot, true);

        uiChooserOfBuilding.ShowMiniHouseCosts(plot, true);

        uiChooserOfBuilding.Show();
        currentPlot = plot;
        isChooserOfBuildingOpen = true;
    }

    public void OnBigBuildingSelected()
    {
        currentPlot.BigBuilding.buildingCore.buildingReward.FinalRentAmount = currentPlot.BRentPrice;
        currentPlot.BigBuilding.buildingCore.buildingReward.FinalRewardAmount= currentPlot.BSellPrice;
        CloseUIChooser();

        currentPlot.state = PlotState.Built;
        currentPlot.BigBuilding.gameObject.SetActive(true);
    }
    public void OnMiniBuildingSelected()
    {
        currentPlot.MiniBuilding.buildingCore.buildingReward.FinalRentAmount = currentPlot.MRentPrice;
        currentPlot.MiniBuilding.buildingCore.buildingReward.FinalRewardAmount = currentPlot.MSellPrice;
        CloseUIChooser();

        currentPlot.state = PlotState.Built;
        currentPlot.MiniBuilding.gameObject.SetActive(true);


    }
}

   

