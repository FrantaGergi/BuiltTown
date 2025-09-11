using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    public GameObject menu;
    private bool isMenuActive = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Menu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isMenuActive = !isMenuActive;
            if (isMenuActive)
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = isMenuActive;
            menu.SetActive(isMenuActive);

            Debug.Log("Menu toggled via Input System" + isMenuActive);
        }
    }
}
