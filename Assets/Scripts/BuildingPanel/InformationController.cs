using System.Collections;
using TMPro;
using UnityEngine;

public class InformationController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform informationPanel;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Animation")]
    [SerializeField, Tooltip("Délka animace pøi zobrazování / skrývání (v sekundách)")] private float animationDuration = 0.18f;
    [SerializeField, Tooltip("Easing použité pro plynulou zmìnu scale (0..1)")] private AnimationCurve ease = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    [Header("Side position")]
    [SerializeField, Tooltip("X souøadnice panelu pokud se má posunout na stranu (toSidePos = true)")] private float sideOffsetX = -400f;

    private Coroutine showCoroutine;

    // pùvodní pozice pro obnovu po zobrazení v boèní pozici
    private Vector2 originalAnchoredPos;
    private bool hasOriginalAnchoredPos = false;
    private bool currentToSidePos = false;


    void Start()
    {
        // Ulož pùvodní anchoredPosition (pokud existuje) a inicializuj panel
        if (informationPanel != null)
        {
            originalAnchoredPos = informationPanel.anchoredPosition;
            hasOriginalAnchoredPos = true;

            informationPanel.localScale = Vector3.zero;
            informationPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Zobrazí panel: plynulá animace scale z 0 -> 1, poèká <duration> sekund, plynulá animace 1 -> 0 a deaktivace.
    /// Volání je odolné proti opakovaným voláním (pøepíše bìžící animaci a zaène znovu plynule).
    /// Pokud je toSidePos==true, panel se pøed animací pøesune na X = sideOffsetX a po skrytí se vrátí na pùvodní pozici.
    /// </summary>
    /// <param name="title">Titulek</param>
    /// <param name="description">Popis</param>
    /// <param name="duration">Doba (v s), po kterou bude panel plnì zobrazený pøed skrytím</param>
    public void ShowText(string title, string description, float duration, bool toSidePos = false)
    {
        if (informationPanel == null)
        {
            Debug.LogWarning("InformationController.ShowText: informationPanel není pøiøazen.");
            return;
        }

        // Aktualizuj texty okamžitì
        if (titleText != null) titleText.text = title ?? string.Empty;
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;

        // Ulož flag pro obnovení pozice po skrytí
        currentToSidePos = toSidePos;

        // Nastav pozici pøed animací
        if (toSidePos && hasOriginalAnchoredPos)
        {
            informationPanel.anchoredPosition = new Vector2(sideOffsetX, originalAnchoredPos.y);
        }
        else if (hasOriginalAnchoredPos)
        {
            informationPanel.anchoredPosition = originalAnchoredPos;
        }

        // Restartuj sekvenci pokud už bìží
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        // Aktivuj panel pøed animací
        informationPanel.gameObject.SetActive(true);

        showCoroutine = StartCoroutine(ShowSequence(Mathf.Max(0f, duration)));
    }

    private IEnumerator ShowSequence(float visibleDuration)
    {

        // Animuj plynule z aktuálního scale na 1
        yield return StartCoroutine(ScaleTo(Vector3.one, animationDuration));

        // Poèkej zadanou dobu (viditelný stav)
        float elapsed = 0f;
        while (elapsed < visibleDuration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Animuj plynule zpìt na 0
        yield return StartCoroutine(ScaleTo(Vector3.zero, animationDuration));

        // Pokud jsme byli v boèní pozici, obnovíme pùvodní anchoredPosition
        if (currentToSidePos && hasOriginalAnchoredPos && informationPanel != null)
        {
            informationPanel.anchoredPosition = originalAnchoredPos;
            currentToSidePos = false;
        }

        // Deaktivuj panel když je zmenšený
        informationPanel.gameObject.SetActive(false);

        showCoroutine = null;
    }

    private IEnumerator ScaleTo(Vector3 target, float duration)
    {
        if (informationPanel == null)
            yield break;

        Vector3 start = informationPanel.localScale;
        float t = 0f;

        // Pokud je duration nulové nebo velmi malé, pøepni okamžitì
        if (duration <= Mathf.Epsilon)
        {
            informationPanel.localScale = target;
            yield break;
        }

        while (t < duration)
        {
            t += Time.deltaTime;
            float norm = Mathf.Clamp01(t / duration);
            float eased = ease != null ? ease.Evaluate(norm) : Mathf.SmoothStep(0f, 1f, norm);
            informationPanel.localScale = Vector3.LerpUnclamped(start, target, eased);
            yield return null;
        }

        informationPanel.localScale = target;
    }

    public void HideInstant()
    {
        // Zastav aktuální animaci
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        // Zastav i pøípadnou animaci ScaleTo
        StopAllCoroutines();

        // Reset scale a pozice
        if (informationPanel != null)
        {
            informationPanel.localScale = Vector3.zero;
            if (hasOriginalAnchoredPos)
                informationPanel.anchoredPosition = originalAnchoredPos;
            informationPanel.gameObject.SetActive(false);
            currentToSidePos = false;
        }
    }
}
