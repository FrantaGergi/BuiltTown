using UnityEngine;
using UnityEngine.InputSystem;
using static UINPC;

public class NPCManager : MonoBehaviour
{
    private bool isNPCManagerOpen = false;
    private string previousActionMap = "";

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject mainNPCManagerContainer;
    [SerializeField] private UINPC UINPC;

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

    public void RegisterNPC(BaseNPC npc, UINPC.Role role, string displayName, string status = "")
    {
        UINPC.RegisterOrUpdateNPC(npc, role, displayName, status);
    }
    public void UpdateNPCStatus(BaseNPC npc, UINPC.Role role, string displayName, string status = "")
    {
        UINPC.RegisterOrUpdateNPC(npc, role, displayName, status);
    }
    public void UnregisterNPC(BaseNPC npc)
    {
        UINPC.UnregisterNPC(npc);
    }

    private void RemoveNPC()
    {

    }

}
