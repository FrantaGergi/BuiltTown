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


    public void ShowMiniHouseCosts(int wood, int stone, int ore, int rent, int sell, bool show)
    {
        mWoodCount.text = wood.ToString();
        mStoneCount.text = stone.ToString();
        mOreCount.text = ore.ToString();
        mRentCount.text = rent.ToString();
        mSellCount.text = sell.ToString();
        miniHouse.gameObject.SetActive(show);
    }
    public void ShowBigHouseCosts(int wood, int stone, int ore, int rent, int sell, bool show)
    {
        bWoodCount.text = wood.ToString();
        bStoneCount.text = stone.ToString();
        bOreCount.text = ore.ToString();
        bRentCount.text = rent.ToString();
        bSellCount.text = sell.ToString();
        bigHouse.gameObject.SetActive(show);
    }

    public void Show()
    {
        content.gameObject.SetActive(true);
    }
    public void Hide()
    {
        content.gameObject.SetActive(false);
    }
}
