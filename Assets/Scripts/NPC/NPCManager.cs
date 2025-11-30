using UnityEngine;
using UnityEngine.InputSystem;

public class NPCManager : MonoBehaviour
{
    private bool isNPCManagerOpen = false;
    private string previousActionMap = "";

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject mainNPCManagerContainer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mainNPCManagerContainer.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ToggleNPCManager(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        isNPCManagerOpen = !isNPCManagerOpen;
        SetNPCManager();

    }
    public void CloseNPCManager()
    {
        isNPCManagerOpen = false;
        SetNPCManager();
    }

    public void SetNPCManager()
    {
        if (isNPCManagerOpen)
        {
            previousActionMap = playerInput.currentActionMap.name;
            playerInput.SwitchCurrentActionMap("UI");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            mainNPCManagerContainer.SetActive(true);
        }
        else
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            mainNPCManagerContainer.SetActive(false);
        }
    }

}
