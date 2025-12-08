
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
        SelectBuildingSite,    // vr t  BuildingSite (vybere Big pokud existuje, jinak Mini)
        SelectWorldPosition    // vr t  sv tovou pozici (nap . zdroj)
    }

    private MinimapSelectionMode selectionMode = MinimapSelectionMode.None;
    private Action<BuildingSite> onBuildingSiteSelected;
    private Action<Vector3> onWorldPositionSelected;

    // nov : zda se minimapa m  po v b ru automaticky zav  t
    private bool _selectionAutoClose = true;



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



        // Pokud jsme v b n m re imu (minimapa otev ena, ale nikdo ne ek  na v b r), nech me existuj c  logiku
        if (isMinimapOpen && selectionMode == MinimapSelectionMode.None)
        {
            plotManager.HandlePlotClick();
            return;
        }

        // Pokud  ek me na v b r, zpracujeme kliknut  jako v b r
        if (isMinimapOpen && selectionMode != MinimapSelectionMode.None)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (plotManager.TryGetPlotFromMinimapScreenPoint(mousePos, out Plot plot, out Vector3 worldPos))
            {
                if (selectionMode == MinimapSelectionMode.SelectWorldPosition)
                {
                    // zachyt me hodnotu auto-close p ed vol n m callbacku (eliminuje race)
                    bool shouldAutoClose = _selectionAutoClose;

                    // vr t me pozici (nap . zdroj)
                    onWorldPositionSelected?.Invoke(worldPos);

                    // Pokud jsme p ed vol n m m li nastaveno autoClose, zav i + vy isti stav.
                    if (shouldAutoClose)
                    {
                        ClearSelectionState();
                        CloseMinimapInternal();
                        Debug.Log($"MinimapManager: vybran  sv tov  pozice {worldPos}.");
                    }
                    // pokud _selectionAutoClose == false, nech me stav (callback m  e hned otev  t nov  re im)
                }
                else if (selectionMode == MinimapSelectionMode.SelectBuildingSite)
                {
                    // Z plotu vybereme preferovan  BuildingSite: prim rn  Big pokud existuje, pak Mini, pak CurrentBuilding
                    BuildingSite site = ChooseBuildingSiteFromPlot(plot);
                    if (site != null)
                    {
                        // zachyt me hodnotu auto-close p ed vol n m callbacku (eliminuje race)
                        bool shouldAutoClose = _selectionAutoClose;

                        onBuildingSiteSelected?.Invoke(site);

                        // Pokud jsme p ed vol n m m li nastaveno autoClose, zav i + vy isti stav.
                        if (shouldAutoClose)
                        {
                            ClearSelectionState();
                            CloseMinimapInternal();
                            Debug.Log($"MinimapManager: vybran  plot {plot.id} s BuildingSite.");
                        }
                        // pokud _selectionAutoClose == false nech me stav a callback m  e zm nit selectionMode / handlery
                    }
                    else
                    {
                        Debug.LogWarning($"MinimapManager: vybran  plot {plot.id} neobsahuje BuildingSite.");
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
            playerInput.SwitchCurrentActionMap(previousActionMap);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            mainMinimapContainer.SetActive(false);
        }
    }

    // PUBLIC API pro NPC / role:
    // Otev e minimapu a  ek  na v b r BuildingSite; po v b ru zavol  callback.
    // param autoClose: pokud true (default), minimapa se po v b ru automaticky zav e; pokud false, z stane otev en .
    public void OpenMinimapToGetBuildingSite(Action<BuildingSite> onSelected, bool autoClose = true)
    {
        selectionMode = MinimapSelectionMode.SelectBuildingSite;
        onBuildingSiteSelected = onSelected;
        onWorldPositionSelected = null;
        _selectionAutoClose = autoClose;
        OpenMinimapForSelection();
    }

    // Otev e minimapu a  ek  na kliknut  na mapu; vr t  sv tovou pozici.
    // param autoClose: pokud true (default), minimapa se po v b ru automaticky zav e.
    public void OpenMinimapToGetSourceCoordinates(Action<Vector3> onSelected, bool autoClose = true)
    {
        selectionMode = MinimapSelectionMode.SelectWorldPosition;
        onWorldPositionSelected = onSelected;
        onBuildingSiteSelected = null;
        _selectionAutoClose = autoClose;
        OpenMinimapForSelection();
    }

    private void OpenMinimapForSelection()
    {
        isMinimapOpen = true;
        SetMinimap();
        // Odstra  aktvn  selection v EventSystem, aby se zabr nilo okam it mu aktivov n  UI element 
        UnityEngine.EventSystems.EventSystem.current?.SetSelectedGameObject(null);
    }

    private void ClearSelectionState()
    {
        selectionMode = MinimapSelectionMode.None;
        onBuildingSiteSelected = null;
        onWorldPositionSelected = null;
        _selectionAutoClose = true;
    }

    // Voliteln  utility / synchronn  z sk n  BuildingSite dle sv tov  pozice
    public BuildingSite GetBuildingSiteByClickedPos(Vector3 pos)
    {
        Plot plot = plotManager?.plotController.GetPlotAtPosition(pos);
        if (plot == null) return null;
        return ChooseBuildingSiteFromPlot(plot);
    }
}
