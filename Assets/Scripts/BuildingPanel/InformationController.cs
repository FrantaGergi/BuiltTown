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

    private Coroutine showCoroutine;

    void Start()
    {
        // Inicializace stavu — panel skrytý a se scale 0, aby byl "vypnutý" po startu.
        if (informationPanel != null)
        {
            informationPanel.localScale = Vector3.zero;
            informationPanel.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Zobrazí panel: plynulá animace scale z 0 -> 1, poèká <duration> sekund, plynulá animace 1 -> 0 a deaktivace.
    /// Volání je odolné proti opakovaným voláním (pøepíše bìžící animaci a zaène znovu plynule).
    /// </summary>
    /// <param name="title">Titulek</param>
    /// <param name="description">Popis</param>
    /// <param name="duration">Doba (v s), po kterou bude panel plnì zobrazený pøed skrytím</param>
    public void ShowText(string title, string description, float duration)
    {
        if (informationPanel == null)
        {
            Debug.LogWarning("InformationController.ShowText: informationPanel není pøiøazen.");
            return;
        }

        // Aktualizuj texty okamžitì
        if (titleText != null) titleText.text = title ?? string.Empty;
        if (descriptionText != null) descriptionText.text = description ?? string.Empty;

        // Restartuj sekvenci pokud už bìží
        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
            showCoroutine = null;
        }

        showCoroutine = StartCoroutine(ShowSequence(Mathf.Max(0f, duration)));
    }

    private IEnumerator ShowSequence(float visibleDuration)
    {
        // Aktivuj panel pøed animací
        informationPanel.gameObject.SetActive(true);

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
}
