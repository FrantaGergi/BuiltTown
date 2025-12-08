using TMPro;
using UnityEngine;
using System.Collections;

public class UIChooserOfBuilding : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform miniHouse;
    [SerializeField] private RectTransform bigHouse;

    [SerializeField] private TextMeshProUGUI mWoodCount;
    [SerializeField] private TextMeshProUGUI mStoneCount;
    [SerializeField] private TextMeshProUGUI mOreCount;
    [SerializeField] private TextMeshProUGUI bWoodCount;
    [SerializeField] private TextMeshProUGUI bStoneCount;
    [SerializeField] private TextMeshProUGUI bOreCount;

    [SerializeField] private TextMeshProUGUI mRentCount;
    [SerializeField] private TextMeshProUGUI mSellCount;
    [SerializeField] private TextMeshProUGUI bRentCount;
    [SerializeField] private TextMeshProUGUI bSellCount;


    public void ShowMiniHouseCosts(Plot plot)
    {
        if (plot.MiniBuilding == null)
        {
            miniHouse.gameObject.SetActive(false);
            return;
        }

        mWoodCount.text = plot.MiniBuilding.buildingCore.woodCost.ToString();
        mStoneCount.text = plot.MiniBuilding.buildingCore.stoneCost.ToString();
        mOreCount.text = plot.MiniBuilding.buildingCore.oreCost.ToString();
        mRentCount.text = plot.MRentPrice.ToString();
        mSellCount.text = plot.MSellPrice.ToString();
        miniHouse.gameObject.SetActive(true);

       // plot.MiniBuilding.buildingCore.buildingReward.FinalAmmountToGive = p
    }
    public void ShowBigHouseCosts(Plot plot)
    {
        if (plot.BigBuilding == null)
        {
            bigHouse.gameObject.SetActive(false);
            return;
        }

        bWoodCount.text = plot.BigBuilding.buildingCore.woodCost.ToString();
        bStoneCount.text = plot.BigBuilding.buildingCore.stoneCost.ToString();
        bOreCount.text = plot.BigBuilding.buildingCore.oreCost.ToString();
        bRentCount.text = plot.BRentPrice.ToString();
        bSellCount.text = plot.BSellPrice.ToString();
        bigHouse.gameObject.SetActive(true);
    }


    public void Hide()
    {
        content.gameObject.SetActive(false);        
    }

    // Pùvodní bezparametrové volání zachováme (bude animovat)
    public void Show()
    {
        content.gameObject.SetActive(true);
    }

   
}
