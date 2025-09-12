using UnityEngine;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private PlayerInput playerInput; // pøipoj PlayerInput z hráèe

    private bool isPaused;
    private string previousActionMap;

    public static PauseManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void TogglePause(InputAction.CallbackContext context)
    {
        if (!context.performed) return; //reaguj jen na performed

        Debug.Log("AHOJjj");



        if (isPaused) ResumeGame();
        else PauseGame();

    }

    public void PauseGame()
    {
        Debug.Log("AHOJjj");
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
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // pøepneme zpìt na gameplay mapu
        if (!string.IsNullOrEmpty(previousActionMap))
            playerInput.SwitchCurrentActionMap(previousActionMap);
    }

    public void OpenSettings()
    {
        // Aktivace settings panelu
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
}
