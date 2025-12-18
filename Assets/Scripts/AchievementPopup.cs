// AchievementPopup.cs - Vyskakující notifikace v pravo dole

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementPopup : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI achievementNameText;
    [SerializeField] private TextMeshProUGUI achievementDescriptionText;
    [SerializeField] private TextMeshProUGUI iconText; // emoji fallback
    [SerializeField] private Image iconImage; // sprite icon
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Icon Database (assign in inspector)")]
    [SerializeField] private AchievementIconDatabase iconDatabase;

    [Header("Animation Settings")]
    [SerializeField] private float slideInDuration = 0.5f;
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float slideOutDuration = 0.5f;
    [SerializeField] private float slideDistance = 300f;
    [SerializeField] private AnimationCurve slideInCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private AnimationCurve slideOutCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Sound")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioSource audioSource;

    private Queue<AchievementService.Achievement> achievementQueue = new Queue<AchievementService.Achievement>();
    private bool isShowingAchievement = false;
    private RectTransform popupRect;
    private Vector2 hiddenPosition;
    private Vector2 visiblePosition;

    private void Awake()
    {
        popupRect = popupPanel.GetComponent<RectTransform>();

        // Vypočítej pozice pro animaci (pravý dolní roh)
        visiblePosition = popupRect.anchoredPosition;
        hiddenPosition = visiblePosition + new Vector2(slideDistance, 0);

        // Nastav na hidden pozici
        popupRect.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0;
        popupPanel.SetActive(false);
    }

    private void OnEnable()
    {
        // Přihlášení k eventu
        AchievementService.OnAchievementUnlocked += OnAchievementUnlocked;
    }

    private void OnDisable()
    {
        AchievementService.OnAchievementUnlocked -= OnAchievementUnlocked;
    }

    private void OnAchievementUnlocked(AchievementService.Achievement achievement)
    {
        achievementQueue.Enqueue(achievement);

        if (!isShowingAchievement)
        {
            StartCoroutine(ShowNextAchievement());
        }
    }

    private IEnumerator ShowNextAchievement()
    {
        while (achievementQueue.Count > 0)
        {
            isShowingAchievement = true;
            var achievement = achievementQueue.Dequeue();

            // Nastav data do UI
            SetupAchievementUI(achievement);

            // Animace: Slide in + Fade in
            yield return StartCoroutine(SlideIn());

            // Zobraz po dobu displayDuration
            yield return new WaitForSeconds(displayDuration);

            // Animace: Slide out + Fade out
            yield return StartCoroutine(SlideOut());
        }

        isShowingAchievement = false;
    }

    private void SetupAchievementUI(AchievementService.Achievement achievement)
    {
        achievementNameText.text = achievement.name;
        achievementDescriptionText.text = achievement.description;

        // Nejprve zkusíme databázi (ScriptableObject) podle id
        Sprite sprite = iconDatabase != null ? iconDatabase.GetSprite(achievement.id) : null;

        // Fallback: Resources/Icons/{id} pokud nechcete DB (volitelné)
        if (sprite == null)
            sprite = Resources.Load<Sprite>($"Icons/{achievement.id}");

        if (sprite != null && iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.preserveAspect = true;
            iconImage.gameObject.SetActive(true);
            if (iconText != null) iconText.gameObject.SetActive(false);
        }
        else
        {
            // Žádná sprite — použij emoji (pokud je)
            if (iconImage != null) iconImage.gameObject.SetActive(false);
            if (iconText != null)
            {
                iconText.text = achievement.icon ?? "";
                iconText.gameObject.SetActive(true);
            }
        }

        // Přehraj zvuk
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
    }

    private IEnumerator SlideIn()
    {
        popupPanel.SetActive(true);
        popupRect.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0;

        float elapsed = 0;
        while (elapsed < slideInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideInDuration;
            float curveT = slideInCurve.Evaluate(t);

            popupRect.anchoredPosition = Vector2.Lerp(hiddenPosition, visiblePosition, curveT);
            canvasGroup.alpha = Mathf.Lerp(0, 1, curveT);

            yield return null;
        }

        popupRect.anchoredPosition = visiblePosition;
        canvasGroup.alpha = 1;
    }

    private IEnumerator SlideOut()
    {
        float elapsed = 0;
        while (elapsed < slideOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / slideOutDuration;
            float curveT = slideOutCurve.Evaluate(t);

            popupRect.anchoredPosition = Vector2.Lerp(visiblePosition, hiddenPosition, curveT);
            canvasGroup.alpha = Mathf.Lerp(1, 0, curveT);

            yield return null;
        }

        popupRect.anchoredPosition = hiddenPosition;
        canvasGroup.alpha = 0;
        popupPanel.SetActive(false);
    }

    // Pro testování v editoru
#if UNITY_EDITOR
    [ContextMenu("Test Achievement Popup")]
    private void TestPopup()
    {
        var testAchievement = new AchievementService.Achievement
        {
            name = "Test Achievement",
            description = "Tohle je testovací achievement!",
            icon = "🎉"
        };
        OnAchievementUnlocked(testAchievement);

        AchievementService.ResetProgress();
    }
#endif
}

/*
 * SETUP NÁVOD:
 * 
 * 1. Vytvoř prázdný GameObject na Canvas (např. "AchievementPopupSystem")
 * 2. Přidej tento script
 * 3. Vytvoř UI strukutru jako child:
 * 
 *    AchievementPopupSystem
 *    └── PopupPanel (Panel/Image s rounded corners)
 *        ├── IconText (TextMeshProUGUI) - pro emoji
 *        ├── AchievementName (TextMeshProUGUI) - bold, větší font
 *        └── Description (TextMeshProUGUI) - menší font
 * 
 * 4. Nastav PopupPanel RectTransform:
 *    - Anchors: Bottom Right (1, 0)
 *    - Pivot: (1, 0)
 *    - Position: (-50, 50) nebo kde chceš v rohu
 * 
 * 5. Přidej CanvasGroup na PopupPanel
 * 6. Přiřaď reference v inspectoru
 * 7. (Optional) Přidej AudioSource a unlock sound
 * 
 * POUŽITÍ:
 * - Script se automaticky přihlásí k AchievementService.OnAchievementUnlocked
 * - Při odemčení achievementu se automaticky zobrazí popup
 * - Pokud se odemkne více achievementů najednou, zobrazí se ve frontě
 */