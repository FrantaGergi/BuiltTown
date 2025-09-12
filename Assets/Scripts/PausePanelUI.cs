using UnityEngine;
using UnityEngine.UI;

public class PausePanelUI : MonoBehaviour
{
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        resumeButton.onClick.AddListener(() => PauseManager.Instance.ResumeGame());
        settingsButton.onClick.AddListener(() => PauseManager.Instance.OpenSettings());
        mainMenuButton.onClick.AddListener(() => PauseManager.Instance.GoToMainMenu());
        closeButton.onClick.AddListener(() => PauseManager.Instance.ClosePanel());
    }
}
