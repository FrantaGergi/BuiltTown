using UnityEngine;
using UnityEngine.InputSystem;

public class DevToolController : MonoBehaviour
{
    public bool devTool = false;
    [SerializeField]
    InformationController informationController;

    // Tato metoda se pøiøadí do Unity Eventu na Ctrl+P
    public void ToggleDevTool()
    {
        devTool = !devTool;
        
        if(devTool)
        {
            informationController.ShowText("DevTool Enabled","Developer tools have been enabled.\nPress the M key to get money.", 5f);
        }
        else
        {
            informationController.ShowText("DevTool Disabled", "Developer tools have been disabled.", 5f);
        }
    }

    // Tato metoda se pøiøadí do Unity Eventu na klávesu M
    public void OnDevToolM(InputAction.CallbackContext context)
    {
        if (!context.performed) return; // Reaguj jen na performed
        if (!devTool) return;

        MoneyManager.Instance.AddMoney(50000);
        // Budoucí obsah se vykoná jen pokud je devTool true
        Debug.Log("DevTool M pressed");
    }
}
