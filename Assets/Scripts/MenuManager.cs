using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private PlayerInput playerInput; // pøipoj PlayerInput z hráèe

    private bool isPaused = false;
    private string previousActionMap;

    public static MenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        if (!context.performed) return; //reaguj jen na performed


        if (isPaused) ResumeGame();
        else PauseGame();

    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // uložíme pøedchozí mapu (obvykle "Player") a pøepneme na UI
        previousActionMap = playerInput.currentActionMap.name;
        playerInput.SwitchCurrentActionMap("UI");
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);


        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // pøepneme zpìt na gameplay mapu
        if (!string.IsNullOrEmpty(previousActionMap))
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);
        }
    }

    public void OpenSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);

    }

    public void GoToMainMenu()
    {
        // Time.timeScale = 1f;
        //  UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    public void ClosePanel()
    {
        ResumeGame();
    }
    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
    }
}
