using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private PlayerInput playerInput; // pøipoj PlayerInput z hráèe

    private bool isPaused = false;
    private string previousActionMap = "Player";

    public static MenuManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    private void Start()
    {
        ResumeGame();
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
        settingsPanel.SetActive(false);
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

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
         Time.timeScale = 1f;
         UnityEngine.SceneManagement.SceneManager.LoadScene("MenuScene");
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

    public void OpenUIEnviroment() 
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;



        playerInput.SwitchCurrentActionMap("UI");
    }
    public void CloseUIEnviroment()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        // pøepneme zpìt na gameplay mapu
        if (!string.IsNullOrEmpty(previousActionMap))
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);
        }
    }
}
