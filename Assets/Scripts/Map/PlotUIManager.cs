using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlotUIManager : MonoBehaviour
{
    public static PlotUIManager Instance { get; private set; }

    [Header("UI Panely")]
    [SerializeField] private GameObject unlockPanel;
    [SerializeField] private GameObject buildingPanel;

    [Header("Unlock Panel")]
    [SerializeField] private TextMeshProUGUI unlockTitleText;
    [SerializeField] private TextMeshProUGUI unlockPriceText;
    [SerializeField] private Button unlockButton;
    [SerializeField] private Button cancelButton;

    [Header("District Panel")]
    [SerializeField] private TextMeshProUGUI buildingTitleText;
    [SerializeField] private Transform buildingButtonContainer;
    [SerializeField] private GameObject buildingButtonPrefab;

    [Header("Game Manager")]
    [SerializeField] private VoronoiGenerator voronoiGenerator;

    private Plot currentPlot;
    private int playerMoney = 10000;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        unlockPanel.SetActive(false);
        buildingPanel.SetActive(false);

        unlockButton.onClick.AddListener(OnUnlockButtonClicked);
        cancelButton.onClick.AddListener(OnCancelButtonClicked);
    }

    public void ShowUnlockDialog(Plot plot)
    {
        currentPlot = plot;

        int price = CalculateUnlockPrice(plot);

        unlockTitleText.text = $"Pozemek #{plot.id}";
        unlockPriceText.text = $"Cena: ${price}";

        unlockButton.interactable = (playerMoney >= price);

        unlockPanel.SetActive(true);
        buildingPanel.SetActive(false);
    }

    public void ShowBuildingMenu(Plot plot)
    {
        currentPlot = plot;

        buildingTitleText.text = $"Stavba na pozemku #{plot.id}";

        // Vygeneruj tlaèítka pro budovy
        foreach (Transform child in buildingButtonContainer)
        {
            Destroy(child.gameObject);
        }

        string[] buildings = { "Dùm", "Obchod", "Továrna", "Park" };
        foreach (string building in buildings)
        {
            GameObject buttonObj = Instantiate(buildingButtonPrefab, buildingButtonContainer);
            Button btn = buttonObj.GetComponent<Button>();
            TextMeshProUGUI btnText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();

            btnText.text = building;
            btn.onClick.AddListener(() => OnBuildingSelected(building));
        }

        unlockPanel.SetActive(false);
        buildingPanel.SetActive(true);
    }

    private void OnUnlockButtonClicked()
    {
        int price = CalculateUnlockPrice(currentPlot);

        if (playerMoney >= price)
        {
            playerMoney -= price;
            voronoiGenerator.UnlockPlot(currentPlot.id);

            Debug.Log($"Pozemek #{currentPlot.id} odemèen!");

            unlockPanel.SetActive(false);
        }
    }

    private void OnCancelButtonClicked()
    {
        unlockPanel.SetActive(false);
        buildingPanel.SetActive(false);
    }

    private void OnBuildingSelected(string buildingName)
    {
        Debug.Log($"Postaveno: {buildingName} na pozemku #{currentPlot.id}");

        // Zde pøidej logiku stavby

        buildingPanel.SetActive(false);
    }

    private int CalculateUnlockPrice(Plot plot)
    {
        // Cena roste s ID pozemku
        return 1000 + (plot.id * 500);
    }
}