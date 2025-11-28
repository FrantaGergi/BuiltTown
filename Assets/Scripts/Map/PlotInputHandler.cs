using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlotInputHandler : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField] private PlotController plotManager;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera minimapCamera;

    [Header("Nastavení")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private bool useMinimapInput = true;

    private Plot selectedPlot;

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
    }

  

   /* public void OnHandlePlotClicked(InputAction.CallbackContext txt)
    {
        if (txt.performed)
        {
            HandlePlotClick();
            Debug.Log("PlotInputHandler: OnHandlePlotClicked performed");
        }
    }
   **/
    private void HandlePlotClick()
    {
        // Kontrola jestli neklikáme na UI
        if (EventSystem.current.IsPointerOverGameObject())
            return;

        // Raycast ze správné kamery
        Camera activeCamera = GetActiveCamera();
        Ray ray = activeCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Plot clickedPlot = plotManager.GetPlotAtPosition(hit.point);

            if (clickedPlot != null)
            {
                if(clickedPlot.state == PlotState.AvailableToUnlock)
                {
                    OnPlotClicked(clickedPlot);
                }
            }
        }
    }

    private Camera GetActiveCamera()
    {
        // Zjisti jestli je kurzor nad minimapou
        if (useMinimapInput && minimapCamera != null)
        {
            Rect minimapRect = minimapCamera.pixelRect;
            if (minimapRect.Contains(Mouse.current.position.ReadValue()))
            {
                return minimapCamera;
            }
        }

        return mainCamera;
    }

    private void OnPlotClicked(Plot plot)
    {
        selectedPlot = plot;

        Debug.Log($"Kliknuto na pozemek #{plot.id}");

        if (!plot.isUnlocked)
        {
            // Zobraz UI pro odemèení
            //PlotUIManager.Instance?.ShowUnlockDialog(plot);
        }
        else
        {
            // Zobraz UI pro stavbu
            //PlotUIManager.Instance?.ShowBuildingMenu(plot);
        }

        plotManager.UnlockPlot(plot.id);
    }

    public Plot GetSelectedPlot() => selectedPlot;
}