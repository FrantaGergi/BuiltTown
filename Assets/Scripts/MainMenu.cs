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
    [SerializeField] private AudioSource audioSource;

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

        //sound
        SoundManager.Instance.PlayOnSource(audioSource, SoundSO.Sound.Background);

        // Pøipoj eventy tak, aby se zmìny okamžitì aplikovaly a uložily
        if (volumeSlider != null)
        {
            volumeSlider.onValueChanged.RemoveAllListeners();
            volumeSlider.onValueChanged.AddListener(v =>
            {
                AudioListener.volume = v;
                PlayerPrefs.SetFloat("Volume", v);
                PlayerPrefs.Save();
            });
        }

        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveAllListeners();
            resolutionDropdown.onValueChanged.AddListener(OnResolutionChange);
        }

        if (qualityDropdown != null)
        {
            qualityDropdown.onValueChanged.RemoveAllListeners();
            qualityDropdown.onValueChanged.AddListener(OnQualityChange);
        }
    }

    // ---------- MAIN MENU ----------

    public void PlayGame()
    {
        SceneManager.LoadScene("MainScene");
    }

    public void OpenSettings()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(true);
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
        if (volumeSlider == null)
        {
            Debug.LogWarning("MainMenu: volumeSlider není pøiøazen.");
            return;
        }
        AudioListener.volume = volumeSlider.value;
        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
        PlayerPrefs.Save();
    }

    // ---------- FULLSCREEN ----------

    public void OnFullscreenToggle()
    {
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;

        // Ulož preferenci hned pøi pøepnutí
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

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
        resolutions = Screen.resolutions ?? new Resolution[0];

        if (resolutionDropdown == null)
        {
            Debug.LogWarning("MainMenu: resolutionDropdown není pøiøazen.");
            return;
        }

        resolutionDropdown.ClearOptions();

        List<string> options = new List<string>();
        int currentIndex = 0;

        // Zobrazíme rozlišení vèetnì refresh rate -> "1920 x 1080 @ 60Hz"
        for (int i = 0; i < resolutions.Length; i++)
        {
            var r = resolutions[i];
            string option = $"{r.width} x {r.height} ({r.refreshRate}Hz)";
            options.Add(option);

            // Najdeme pøesnou shodu vèetnì refresh rate
            if (r.width == Screen.currentResolution.width &&
                r.height == Screen.currentResolution.height &&
                r.refreshRate == Screen.currentResolution.refreshRate)
            {
                currentIndex = i;
            }
        }

        if (options.Count == 0)
        {
            // Fallback na current resolution pokud nic jiného není
            var cr = Screen.currentResolution;
            options.Add($"{cr.width} x {cr.height} ({cr.refreshRate}Hz)");
            currentIndex = 0;
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = Mathf.Clamp(currentIndex, 0, options.Count - 1);
        resolutionDropdown.RefreshShownValue();
    }

    public void OnResolutionChange(int index)
    {
        if (resolutions == null || resolutions.Length == 0)
        {
            Debug.LogWarning("MainMenu: nejsou dostupné žádné rozlišení.");
            return;
        }

        if (index < 0 || index >= resolutions.Length)
        {
            Debug.LogWarning($"MainMenu: index rozlišení {index} mimo rozsah.");
            return;
        }

        Resolution res = resolutions[index];
        // Použijeme overload SetResolution s preferovaným refresh rate
        Screen.SetResolution(res.width, res.height, Screen.fullScreen, res.refreshRate);
        PlayerPrefs.SetInt("Resolution", index);
        PlayerPrefs.Save();
    }

    // ---------- QUALITY ----------

    private void InitQuality()
    {
        if (qualityDropdown == null)
        {
            Debug.LogWarning("MainMenu: qualityDropdown není pøiøazen.");
            return;
        }

        qualityDropdown.ClearOptions();
        qualityDropdown.AddOptions(new List<string>(QualitySettings.names));
        qualityDropdown.value = QualitySettings.GetQualityLevel();
        qualityDropdown.RefreshShownValue();
    }

    public void OnQualityChange(int index)
    {
        QualitySettings.SetQualityLevel(index);
        PlayerPrefs.SetInt("Quality", index);
        PlayerPrefs.Save();
    }

    // ---------- SAVE / LOAD ----------

    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("Volume", volumeSlider != null ? volumeSlider.value : AudioListener.volume);
        PlayerPrefs.SetInt("Fullscreen", IsFullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadSettings()
    {
        // Volume
        float volume = PlayerPrefs.GetFloat("Volume", 1f);
        if (volumeSlider != null)
        {
            volumeSlider.SetValueWithoutNotify(volume);
        }
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
            Vector2 pos = rect.anchoredPosition;
            rect.anchoredPosition = new Vector2(targetX, pos.y);
        }

        // Resolution
        int resIndex = PlayerPrefs.GetInt("Resolution", (resolutionDropdown != null ? resolutionDropdown.value : 0));
        if (resolutionDropdown != null)
        {
            // don't set an out-of-range index
            if (resolutions != null && resolutions.Length > 0)
                resIndex = Mathf.Clamp(resIndex, 0, resolutions.Length - 1);

            resolutionDropdown.SetValueWithoutNotify(Mathf.Clamp(resIndex, 0, resolutionDropdown.options.Count - 1));
            resolutionDropdown.RefreshShownValue();

            // apply resolution only if valid
            if (resolutions != null && resolutions.Length > 0 && resIndex >= 0 && resIndex < resolutions.Length)
                OnResolutionChange(resIndex);
        }

        // Quality
        int quality = PlayerPrefs.GetInt("Quality", QualitySettings.GetQualityLevel());
        if (qualityDropdown != null)
        {
            quality = Mathf.Clamp(quality, 0, QualitySettings.names.Length - 1);
            qualityDropdown.SetValueWithoutNotify(quality);
            qualityDropdown.RefreshShownValue();
        }
        QualitySettings.SetQualityLevel(quality);
    }

    // ---------- VISUAL HELPERS ----------

    private IEnumerator MoveAndColor(Image img, float targetX, Color targetColor)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        RectTransform rect = img.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, startPos.y);
        Color startColor = img.color;

        while (elapsed < lerpDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / lerpDuration));

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            img.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        rect.anchoredPosition = endPos;
        img.color = targetColor;
    }

    private void ShowMainMenu()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (settingsPanel != null) settingsPanel.SetActive(false);
    }
    public void OnDestroy()
    {
        Destroy(this.gameObject);
    }
}
