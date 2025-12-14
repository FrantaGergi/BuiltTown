using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Settings Controls")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Toggle fullscreenToggle;

    private void Start()
    {
        // Zobraz hlavní menu a skryj nastavení
        ShowMainMenu();

        // Naèti uložená nastavení
        LoadSettings();
    }

    // --- MAIN MENU BUTTONS ---

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // --- SETTINGS MENU ---

    public void BackToMainMenu()
    {
        SaveSettings();
        ShowMainMenu();
    }

    public void OnVolumeChange()
    {
        AudioListener.volume = volumeSlider.value;
    }

    public void OnFullscreenToggle()
    {
        Screen.fullScreen = fullscreenToggle.isOn;
    }

    // --- HELPER METHODS ---

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Naèti hlasitost (výchozí 1.0)
        float volume = PlayerPrefs.GetFloat("Volume", 1.0f);
        volumeSlider.value = volume;
        AudioListener.volume = volume;

        // Naèti fullscreen (výchozí true)
        bool fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        fullscreenToggle.isOn = fullscreen;
        Screen.fullScreen = fullscreen;
    }
}