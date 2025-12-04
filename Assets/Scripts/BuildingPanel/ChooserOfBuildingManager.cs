using UnityEngine;
using UnityEngine.InputSystem;

public class ChooserOfBuildingManager : MonoBehaviour
{
    private bool isChooserOfBuildingOpen = false;
    private string previousActionMap = "";

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private UIChooserOfBuilding uiChooserOfBuilding;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        uiChooserOfBuilding.Hide();
    }

    public void CloseUIChooser()
    {
        playerInput.SwitchCurrentActionMap(previousActionMap);

    }

    public void SetBuildingChooser()
        {
        if(isChooserOfBuildingOpen)
        {
            previousActionMap = playerInput.currentActionMap.name;
            playerInput.SwitchCurrentActionMap("UI");
            uiChooserOfBuilding.Show();
        }
        else
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);
            uiChooserOfBuilding.Hide();
        }

    }

    


}
