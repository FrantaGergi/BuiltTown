using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("Audio")]
    [SerializeField] private Slider volumeSlider;

    [Header("Graphics")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private TMP_Dropdown qualityDropdown;

    [Header("Fullscreen Toggle Visual")]
    [SerializeField] private float toggleXFalse = -45f;
    [SerializeField] private float toggleXTrue = 45f;
    [SerializeField] private float lerpDuration = 0.3f;
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private Image toggleImage;

    private Coroutine toggleRoutine;
    private Resolution[] resolutions;
    private bool isFullscreen = true;

    private bool IsFullscreen => isFullscreen;

    // ---------- UNITY ----------

    private void Start()
    {
        ShowMainMenu();
        InitResolutions();
        InitQuality();
        LoadSettings();
    }

    // ---------- MAIN MENU ----------

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void BackToMainMenu()
    {
        SaveSettings();
        ShowMainMenu();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // ---------- AUDIO ----------

    public void OnVolumeChange()
    {
        AudioListener.volume = volumeSlider.value;
    }

    // ---------- FULLSCREEN ----------

    public void OnFullscreenToggle()
    {
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;

        float targetX = isFullscreen ? toggleXTrue : toggleXFalse;
        Color targetColor = isFullscreen ? activeColor : inactiveColor;

        if (toggleRoutine != null)
            StopCoroutine(toggleRoutine);

        toggleRoutine = StartCoroutine(
            MoveAndColor(toggleImage, targetX, targetColor)
        );
    }

    // ---------- RESOLUTION ----------

    private void InitResolutions()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width &&
                resolutions[i].height == Screen.currentResolution.height)
            {
                currentIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void OnResolutionChange(int index)
    {
        Resolution res = resolutions[index];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
        PlayerPrefs.SetInt("Resolution", index);
    }

    // ---------- QUALITY ----------

    private void InitQuality()
    {
        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    public void OnQualityChange(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
    }

    // ---------- SAVE / LOAD ----------

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", IsFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Volume
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        volumeSlider.value = volume;
        AudioListener.volume = volume;

        // Fullscreen
        isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = isFullscreen;

        float targetX = isFullscreen ? toggleXTrue : toggleXFalse;
        Color targetColor = isFullscreen ? activeColor : inactiveColor;

        if (toggleImage != null)
        {
            toggleImage.color = targetColor;
            RectTransform rect = toggleImage.rectTransform;
            Vector3 pos = rect.localPosition;
            rect.localPosition = new Vector3(targetX, pos.y, pos.z);
        }

        // Resolution
        int resIndex = PlayerPrefs.GetInt("Resolution", resolutionDropdown.value);
        resolutionDropdown.value = resIndex;
        OnResolutionChange(resIndex);

        // Quality
        int quality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        qualityDropdown.value = quality;
        QualitySettings.SetQualityLevel(quality);
    }

    // ---------- VISUAL HELPERS ----------

    private IEnumerator MoveAndColor(Image img, float targetX, Color targetColor)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        RectTransform rect = img.rectTransform;
        Vector3 startPos = rect.localPosition;
        Vector3 endPos = new Vector3(targetX, startPos.y, startPos.z);
        Color startColor = img.color;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / lerpDuration);

            rect.localPosition = Vector3.Lerp(startPos, endPos, t);
            img.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        rect.localPosition = endPos;
        img.color = targetColor;
    }

    private void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
    }
}
