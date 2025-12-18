using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseController : MonoBehaviour
{
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
        InitResolutions();
        InitQuality();
        LoadSettings();

        // Pøipojíme eventy, aby se zmìny ihned uložily a aplikovaly
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
        Debug.Log("Toggling fullscreen");
        isFullscreen = !isFullscreen;
        Screen.fullScreen = isFullscreen;

        if (toggleImage == null)
        {
            Debug.LogWarning("PauseController: toggleImage není pøiøazen.");
            return;
        }

        // Ulož preferenci hned pøi pøepnutí
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
        PlayerPrefs.Save();

        float targetX = isFullscreen ? toggleXTrue : toggleXFalse;
        Color targetColor = isFullscreen ? activeColor : inactiveColor;

        if (toggleRoutine != null)
        {
            StopCoroutine(toggleRoutine);
            toggleRoutine = null;
        }

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

        if (options.Count == 0)
        {
            // Fallback na current resolution pokud nic jiného není
            options.Add(Screen.currentResolution.width + " x " + Screen.currentResolution.height);
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
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
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

        Debug.Log("Toggle animation started");
        float elapsed = 0f;
        RectTransform rect = img.rectTransform;
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = new Vector2(targetX, startPos.y);
        Color startColor = img.color;

        // use unscaled delta time so animation runs while Time.timeScale == 0 (pause)
        while (elapsed < lerpDuration)
        {
            float delta = Time.unscaledDeltaTime;
            elapsed += delta;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / lerpDuration));

            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            img.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        rect.anchoredPosition = endPos;
        img.color = targetColor;
        toggleRoutine = null;
        Debug.Log("Toggle animation complete");
    }

}