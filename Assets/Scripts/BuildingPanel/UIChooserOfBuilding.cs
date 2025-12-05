using TMPro;
using UnityEngine;

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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }


    public void ShowMiniHouseCosts(Plot plot, bool show)
    {
        if (plot.MiniBuilding == null)
            return;

        mWoodCount.text = plot.MiniBuilding.buildingCore.woodCost.ToString();
        mStoneCount.text = plot.MiniBuilding.buildingCore.stoneCost.ToString();
        mOreCount.text = plot.MiniBuilding.buildingCore.oreCost.ToString();
        mRentCount.text = plot.MRentPrice.ToString();
        mSellCount.text = plot.MSellPrice.ToString();
        miniHouse.gameObject.SetActive(show);

       // plot.MiniBuilding.buildingCore.buildingReward.FinalAmmountToGive = p
    }
    public void ShowBigHouseCosts(Plot plot, bool show)
    {
        if (plot.BigBuilding == null)
            return;



        bWoodCount.text = plot.BigBuilding.buildingCore.woodCost.ToString();
        bStoneCount.text = plot.BigBuilding.buildingCore.stoneCost.ToString();
        bOreCount.text = plot.BigBuilding.buildingCore.oreCost.ToString();
        bRentCount.text = plot.BRentPrice.ToString();
        bSellCount.text = plot.BSellPrice.ToString();
        bigHouse.gameObject.SetActive(show);
    }

   
    public void Hide()
    {
        content.gameObject.SetActive(false);
    }
    public void Show()
        {
        content.gameObject.SetActive(true);
    }

    public void OnSelectedBigHouse()
    {
        Debug.Log("Big house selected");
    }
    public void OnSelectedMiniHouse()
    {
        Debug.Log("Mini house selected");
    }
}
