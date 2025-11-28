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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       mainMinimapContainer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnClicked(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if(isMinimapOpen)
            plotManager.HandlePlotClick();
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


}
