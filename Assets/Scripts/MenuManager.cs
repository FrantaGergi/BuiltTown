using UnityEngine;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isMenuActive = !isMenuActive;

            if(isMenuActive)
            Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;

            Cursor.visible = isMenuActive;
            menu.SetActive(isMenuActive);
        }
    }
}
