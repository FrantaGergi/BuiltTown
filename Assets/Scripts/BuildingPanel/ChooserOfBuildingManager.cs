using UnityEngine;
using UnityEngine.InputSystem;

public class ChooserOfBuildingManager : MonoBehaviour
{
    private bool isChooserOfBuildingOpen = false;

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
        uiChooserOfBuilding.Hide();

    }

    public void OpenBuildingChooser(Plot plot)
    {
        if (plot.BigBuilding != null)
            uiChooserOfBuilding.ShowBigHouseCosts(plot, true);

        if (plot.MiniBuilding != null)
            uiChooserOfBuilding.ShowMiniHouseCosts(plot, true);

        uiChooserOfBuilding.Show();
    }

}

   

