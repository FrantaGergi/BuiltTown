using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlotManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] public PlotController plotController;
    [SerializeField] public ChooserOfBuildingManager chooserOfBuildingManager;
    [SerializeField] private Camera minimapCamera;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] public YesOrNoController yesOrNoController;
    [SerializeField] private RawImage minimapImage;
    [SerializeField] private InformationController informationController;
    [SerializeField] private MinimapIconManager miniIManager;


    [Header("Settings")]
    [SerializeField] private LayerMask groundLayer;


    private Action onYesButtonClicked;

    private Plot selectedPlot;

    public bool IsYereOrNoPanelEnabled => yesOrNoController.YesOrNoPanelEnabled;




    private void Start()
    {
        onYesButtonClicked += () =>
        {
            if (MoneyManager.Instance.TrySpend(selectedPlot.costToUnlock))
            {
                AchievementService.OnDistrictPurchased();

                plotController.UnlockPlot(selectedPlot.id);
                Debug.Log($"Pozemek #{selectedPlot.id} odemèen.");
                chooserOfBuildingManager.OpenBuildingChooser(selectedPlot);
            }
            else
            {
                informationController.ShowText("Not enough money", "You do not have enough money to unlock this plot.", 2f);
            }
        };  
    }
    public void HandlePlotClick()
    {
        if(plotController.gameObject.activeSelf == false ||
            chooserOfBuildingManager.isChooserOfBuildingOpen ||
            yesOrNoController.YesOrNoPanelEnabled || !miniIManager.DistrictsVisible)
        {
            return;
        }

        Vector2 mousePos = Mouse.current.position.ReadValue();

        // 1) Zkontroluj, zda klik je uvnitø UI minimapy
        if (!RectTransformUtility.RectangleContainsScreenPoint(minimapImage.rectTransform, mousePos))
        {
            Debug.Log("Click NOT on minimap UI.");
            return;
        }

        // 2) Screen  Local point minimapy
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapImage.rectTransform,
            mousePos,
            null,
            out Vector2 localPoint
        );

        Rect rect = minimapImage.rectTransform.rect;

        // 3) Local  Normalized UV (0–1)
        float u = (localPoint.x - rect.x) / rect.width;
        float v = (localPoint.y - rect.y) / rect.height;

        // 4) UV Ray
        Ray ray = minimapCamera.ViewportPointToRay(new Vector3(u, v, 0));

        // 5) Raycast
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {

            Plot clickedPlot = plotController.GetPlotAtPosition(hit.point);
            if (clickedPlot != null)
            {
                OnPlotClicked(clickedPlot);
            }
        }
       
    }


    private void OnPlotClicked(Plot plot)
    {


        selectedPlot = plot;

        Debug.Log($"Kliknuto na pozemek #{plot.id}");

        if (plot.state == PlotState.Unlocked)
        {
            // Zobraz UI pro odemèení
            //PlotUIManager.Instance?.ShowUnlockDialog(plot);
            chooserOfBuildingManager.OpenBuildingChooser(plot);
            

        }
        if ( plot.state == PlotState.Built)
        {
            // Zobraz UI pro stavbu
            //PlotUIManager.Instance?.ShowBuildingMenu(plot);
        }else if (plot.state == PlotState.AvailableToUnlock)
        {
            string mess = $"Buy distinct #{plot.id} for ${plot.costToUnlock}?";
            yesOrNoController.Show(mess, onYesButtonClicked);
           

        }
        
     
    }

    // Nová veøejná utilita: z obrazových souøadnic (screen) zkusíme získat plot a svìtovou pozici na ground layer
    public bool TryGetPlotFromMinimapScreenPoint(Vector2 screenPoint, out Plot plot, out Vector3 worldPoint)
    {
        plot = null;
        worldPoint = Vector3.zero;

        if (minimapImage == null || minimapCamera == null || plotController == null)
            return false;

        // Kontrola, že klik je uvnitø minimapy
        if (!RectTransformUtility.RectangleContainsScreenPoint(minimapImage.rectTransform, screenPoint))
            return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapImage.rectTransform,
            screenPoint,
            null,
            out Vector2 localPoint
        );

        Rect rect = minimapImage.rectTransform.rect;
        float u = (localPoint.x - rect.x) / rect.width;
        float v = (localPoint.y - rect.y) / rect.height;

        Ray ray = minimapCamera.ViewportPointToRay(new Vector3(u, v, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            worldPoint = hit.point;
            plot = plotController.GetPlotAtPosition(hit.point);
            return true;
        }

        return false;
    }

    public Plot GetSelectedPlot() => selectedPlot;

}
