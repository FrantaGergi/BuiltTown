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
    [SerializeField] private InformationController informationController;
    [SerializeField] private NPCManager npcManager;

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

    // novì: zda se minimapa má po výbìru automaticky zavøít
    private bool _selectionAutoClose = true;

    // novì: ignorovat první klik(y) po otevøení minimapy (vyøeší "prokliknutí" které otevøelo minimapu)
    [Header("Anti?double-click")]
    [SerializeField, Tooltip("Poèet sekund, po které ignorujeme první klik po otevøení minimapy")] private float ignoreClickWindow = 0.08f;
    private float _ignoreClicksUntil = 0f;


    public bool IsMinimapOpen => isMinimapOpen;
    public bool IsChooserActive => plotManager.chooserOfBuildingManager.isChooserOfBuildingOpen;

    public bool IsYesOrNoPanelActive => plotManager.IsYereOrNoPanelEnabled;

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

        // Ignoruj kliknutí které pøišlo pøíliš brzy po otevøení minimapy
        if (Time.unscaledTime < _ignoreClicksUntil)
        {
            Debug.Log("MinimapManager: ignored click because it's within ignore window after opening minimap.");
            return;
        }

        // Pokud jsme v bìžném režimu (minimapa otevøena, ale nikdo neèeká na výbìr), necháme existující logiku
        if (isMinimapOpen && selectionMode == MinimapSelectionMode.None)
        {
            plotManager.HandlePlotClick();
            return;
        }

        // Pokud èekáme na výbìr, zpracujeme kliknutí jako výbìr
        if (isMinimapOpen && selectionMode != MinimapSelectionMode.None)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (plotManager.TryGetPlotFromMinimapScreenPoint(mousePos, out Plot plot, out Vector3 worldPos))
            {
                informationController.HideInstant();
                if (selectionMode == MinimapSelectionMode.SelectWorldPosition)
                {
                    // zachytíme hodnotu auto-close pøed voláním callbacku (eliminuje race)
                    bool shouldAutoClose = _selectionAutoClose;

                    // vrátíme pozici (napø. zdroj)
                    onWorldPositionSelected?.Invoke(worldPos);

                    // Pokud jsme pøed voláním mìli autoClose, zavøít + vyèistit stav.
                    if (shouldAutoClose)
                    {
                        ClearSelectionState();
                        CloseMinimapInternal();
                        Debug.Log($"MinimapManager: vybraná svìtová pozice {worldPos}.");
                    }
                    // pokud _selectionAutoClose == false, necháme stav (callback mùže hned otevøít nový režim)
                }
                else if (selectionMode == MinimapSelectionMode.SelectBuildingSite)
                {
                    // Z plotu vybereme preferovanou BuildingSite: primárnì Big pokud existuje, pak Mini, pak CurrentBuilding
                    BuildingSite site = ChooseBuildingSiteFromPlot(plot);
                    if (site != null)
                    {
                        bool shouldAutoClose = _selectionAutoClose;

                        onBuildingSiteSelected?.Invoke(site);

                        if (shouldAutoClose)
                        {
                            ClearSelectionState();
                            CloseMinimapInternal();
                            Debug.Log($"MinimapManager: vybraný plot {plot?.id} s BuildingSite.");
                        }
                        // pokud _selectionAutoClose == false necháme stav a callback mùže mìnit selectionMode / handlery
                    }
                    else
                    {
                        informationController.ShowText("No building", $"District #{plot?.id} has no building.\n " +
                            "If you dont have, close this page \n" +
                            "and open minimap to setup building", 8f, true);
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
        if (plot.BigBuilding != null && plot.BigBuilding.gameObject.activeSelf) return plot.BigBuilding;
        if (plot.MiniBuilding != null && plot.MiniBuilding.gameObject.activeSelf) return plot.MiniBuilding;
        if (plot.CurrentBuilding != null && plot.CurrentBuilding.gameObject.activeSelf) return plot.CurrentBuilding;
        return null;
    }

    public void ToggleMinimap(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (plotManager.IsYereOrNoPanelEnabled)
        {
            plotManager.yesOrNoController.Hide();
            return;
        }

        else if(plotManager.chooserOfBuildingManager.isChooserOfBuildingOpen)
        {
            plotManager.chooserOfBuildingManager.CloseUIChooser();
            return;
        }
         else if(npcManager.ISNPCManagerOpen && !isMinimapOpen)
        {
            npcManager.CloseNPCManager();
        }

        isMinimapOpen = !isMinimapOpen;
        SetMinimap();

        // pøi otevøení pøes toggle ignorujeme krátké následující kliknutí
        if (isMinimapOpen)
            _ignoreClicksUntil = Time.unscaledTime + ignoreClickWindow;
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
        if (isMinimapOpen)
        {
            if (previousActionMap == "")
                previousActionMap = playerInput.currentActionMap.name;
            playerInput.SwitchCurrentActionMap("UI");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            mainMinimapContainer.SetActive(true);
        }
        else
        {
            // pokud zavíráme minimapu, zajistíme, aby se jakýkoli aktivní selection ukonèil
            ClearSelectionState();

            if(npcManager.ISNPCManagerOpen == false)
            {
                playerInput.SwitchCurrentActionMap(previousActionMap);

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            mainMinimapContainer.SetActive(false);
            informationController.HideInstant();

        }
    }

    // PUBLIC API pro NPC / role:
    // Otevøe minimapu a èeká na výbìr BuildingSite; po výbìru zavolá callback.
    // param autoClose: pokud true (default), minimapa se po výbìru automaticky zavøe; pokud false, zùstane otevøená.
    public void OpenMinimapToGetBuildingSite(Action<BuildingSite> onSelected, bool autoClose = true)
    {
        selectionMode = MinimapSelectionMode.SelectBuildingSite;
        onBuildingSiteSelected = onSelected;
        onWorldPositionSelected = null;
        _selectionAutoClose = autoClose;
        OpenMinimapForSelection();
        informationController.ShowText("Choose district", "Click on the district \n" +
            " you want to assign him", 50f, true);
    }

    // Otevøe minimapu a èeká na kliknutí na mapu; vrátí svìtovou pozici.
    // param autoClose: pokud true (default), minimapa se po výbìru automaticky zavøe.
    public void OpenMinimapToGetSourceCoordinates(Action<Vector3> onSelected, bool autoClose = true)
    {
        selectionMode = MinimapSelectionMode.SelectWorldPosition;
        onWorldPositionSelected = onSelected;
        onBuildingSiteSelected = null;
        _selectionAutoClose = autoClose;
        OpenMinimapForSelection();
        informationController.ShowText("Choose location", "Click on the map \n" +
            " to select the location", 50f, true);
    }

    private void OpenMinimapForSelection()
    {
        isMinimapOpen = true;
        SetMinimap();
        // Odstraò aktivní selection v EventSystem, aby se zabránilo okamžitému aktivování UI elementù
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);

        // ignoruj první klik krátce po otevøení (vyøeší "prokliknutí")
        _ignoreClicksUntil = Time.unscaledTime + ignoreClickWindow;
    }

    private void ClearSelectionState()
    {
        selectionMode = MinimapSelectionMode.None;
        onBuildingSiteSelected = null;
        onWorldPositionSelected = null;
        _selectionAutoClose = true;
    }

    // Volitelné utility / synchronní získání BuildingSite dle svìtové pozice
    public BuildingSite GetBuildingSiteByClickedPos(Vector3 pos)
    {
        Plot plot = plotManager?.plotController.GetPlotAtPosition(pos);
        if (plot == null) return null;
        return ChooseBuildingSiteFromPlot(plot);
    }
}
