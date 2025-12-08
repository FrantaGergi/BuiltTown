using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MinimapManager : MonoBehaviour
{
    private bool isMinimapOpen = false;
    private string previousActionMap = "";

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject mainMinimapContainer;
    [SerializeField] private PlotManager plotManager;

    // Selection state for "open minimap to choose something"
    private enum MinimapSelectionMode
    {
        None,
        SelectBuildingSite,    // vrátí BuildingSite (vybere Big pokud existuje, jinak Mini)
        SelectWorldPosition    // vrátí svìtovou pozici (napø. zdroj)
    }

    private MinimapSelectionMode selectionMode = MinimapSelectionMode.None;
    private Action<BuildingSite> onBuildingSiteSelected;
    private Action<Vector3> onWorldPositionSelected;

    void Start()
    {
       mainMinimapContainer.SetActive(false);
    }

    void Update()
    {
        
    }

    public void OnClicked(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        // Pokud jsme v bìžném režimu (minimapa otevøena, ale nikdo neèeká na výbìr), necháme existující logiku
        if(isMinimapOpen && selectionMode == MinimapSelectionMode.None)
            plotManager.HandlePlotClick();

        // Pokud èekáme na výbìr, zpracujeme kliknutí jako výbìr
        if (isMinimapOpen && selectionMode != MinimapSelectionMode.None)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (plotManager.TryGetPlotFromMinimapScreenPoint(mousePos, out Plot plot, out Vector3 worldPos))
            {
                if (selectionMode == MinimapSelectionMode.SelectWorldPosition)
                {
                    // vrátíme pozici (napø. zdroj)
                    onWorldPositionSelected?.Invoke(worldPos);
                    ClearSelectionState();
                    CloseMinimapInternal();
                }
                else if (selectionMode == MinimapSelectionMode.SelectBuildingSite)
                {
                    // Z plotu vybereme preferovaný BuildingSite: primárnì Big pokud existuje, pak Mini, pak CurrentBuilding
                    BuildingSite site = ChooseBuildingSiteFromPlot(plot);
                    if (site != null)
                    {
                        onBuildingSiteSelected?.Invoke(site);
                        ClearSelectionState();
                        CloseMinimapInternal();
                    }
                    else
                    {
                        Debug.LogWarning($"MinimapManager: vybraný plot {plot.id} neobsahuje BuildingSite.");
                    }
                }
            }
            else
            {
                Debug.Log("MinimapManager: klik mimo ground hit nebo mimo minimapu.");
            }
        }
    }

    private BuildingSite ChooseBuildingSiteFromPlot(Plot plot)
    {
        if (plot == null) return null;
        // Prefer big site if present, pak mini, pak current
        if (plot.BigBuilding != null) return plot.BigBuilding;
        if (plot.MiniBuilding != null) return plot.MiniBuilding;
        if (plot.CurrentBuilding != null) return plot.CurrentBuilding;
        return null;
    }

    public void ToggleMinimap(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        isMinimapOpen = !isMinimapOpen;
        SetMinimap();
        
    }

    public void CloseMinimap()
    {
        isMinimapOpen = false;
        SetMinimap();
        ClearSelectionState();
    }

    private void CloseMinimapInternal()
    {
        isMinimapOpen = false;
        SetMinimap();
    }

    public void SetMinimap()
    {
        if(isMinimapOpen)
        {
            previousActionMap = playerInput.currentActionMap.name;
            playerInput.SwitchCurrentActionMap("UI");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            mainMinimapContainer.SetActive(true);
        }
        else
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            mainMinimapContainer.SetActive(false);
        }
    }

    // PUBLIC API pro NPC / role:
    // Otevøe minimapu a èeká na výbìr BuildingSite; po výbìru zavolá callback.
    public void OpenMinimapToGetBuildingSite(Action<BuildingSite> onSelected)
    {
        selectionMode = MinimapSelectionMode.SelectBuildingSite;
        onBuildingSiteSelected = onSelected;
        onWorldPositionSelected = null;
        OpenMinimapForSelection();
    }

    // Otevøe minimapu a èeká na kliknutí na mapu; vrátí svìtovou pozici.
    public void OpenMinimapToGetSourceCoordinates(Action<Vector3> onSelected)
    {
        selectionMode = MinimapSelectionMode.SelectWorldPosition;
        onWorldPositionSelected = onSelected;
        onBuildingSiteSelected = null;
        OpenMinimapForSelection();
    }

    private void OpenMinimapForSelection()
    {
        isMinimapOpen = true;
        SetMinimap();
        // Odstraò aktvní selection v EventSystem, aby se zabránilo okamžitému aktivování UI elementù
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    private void ClearSelectionState()
    {
        selectionMode = MinimapSelectionMode.None;
        onBuildingSiteSelected = null;
        onWorldPositionSelected = null;
    }

    // Volitelné utility / synchronní získání BuildingSite dle svìtové pozice
    public BuildingSite GetBuildingSiteByClickedPos(Vector3 pos)
    {
        Plot plot = plotManager?.plotController.GetPlotAtPosition(pos);
        if (plot == null) return null;
        return ChooseBuildingSiteFromPlot(plot);
    }
}
