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

    // true pokud informationPanel.gameObject == this.gameObject (controller je umístìn pøímo na panelu)
    private bool panelIsSelf = false;

    // volitelný CanvasGroup pro plynulé ovládání interaktivity, pokud je pøítomen
    private CanvasGroup panelCanvasGroup;

    private void Awake()
    {
        if (informationPanel == null)
        {
            Debug.LogWarning("InformationController: informationPanel není pøiøazen v Inspectoru.");
            return;
        }

        panelIsSelf = (informationPanel.gameObject == this.gameObject);

        // uložíme pùvodní pozici (anchored) pokud existuje
        originalAnchoredPos = informationPanel.anchoredPosition;
        hasOriginalAnchoredPos = true;

        // pokusíme se najít CanvasGroup, pokud existuje
        panelCanvasGroup = informationPanel.GetComponent<CanvasGroup>();

        // Pokud je controller na tomtéž GameObjectu, NEDÌLÁME SetActive(false)
        // protože to deaktivuje komponentu a StartCoroutine by selhal.
        // Staèí nastavit scale na 0 a pøípadnì canvasGroup interaktivitu.
        if (panelIsSelf)
        {
            informationPanel.localScale = Vector3.zero;
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
        }
        else
        {
            // pokud controller není na stejném GO, je bezpeèné panel deaktivovat
            informationPanel.localScale = Vector3.zero;
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
            informationPanel.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        // Start zùstává volný
    }

    /// <summary>
    /// Zobrazí panel: plynulá animace scale z 0 -> 1, poèká <duration> sekund, plynulá animace 1 -> 0 a deaktivace.
    /// Volání je odolné proti opakovaným voláním (pøepíše bìžící animaci a zaène znovu plynule).
    /// Pokud je toSidePos==true, panel se pøed animací pøesune na X = sideOffsetX a po skrytí se vrátí na pùvodní pozici.
    /// </summary>
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

        // Aktivace vizuálu:
        if (!panelIsSelf)
        {
            informationPanel.gameObject.SetActive(true);
        }
        else
        {
            // pokud jsme na stejném GO, jen upravíme CanvasGroup a scale pøed animací
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 1f; // bude animováno scale - nastavíme alfa na 1 aby byl vidìt
                panelCanvasGroup.interactable = true;
                panelCanvasGroup.blocksRaycasts = true;
            }
        }

        // Ujistíme se, že scale zaèíná od nuly (pokud ne, necháme aktuální)
        if (informationPanel.localScale == Vector3.zero)
            informationPanel.localScale = Vector3.zero;

        // StartCoroutine lze bezpeènì volat, protože tento MonoBehaviour je aktivní
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

        // Deaktivace / ukonèení viditelnosti
        if (!panelIsSelf)
        {
            informationPanel.gameObject.SetActive(false);
        }
        else
        {
            // pokud jsme na stejném GO, ponecháme GO aktivní, ale zajistíme, že je invisible / neinteraktivní
            if (panelCanvasGroup != null)
            {
                panelCanvasGroup.alpha = 0f;
                panelCanvasGroup.interactable = false;
                panelCanvasGroup.blocksRaycasts = false;
            }
        }

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

            if (!panelIsSelf)
            {
                informationPanel.gameObject.SetActive(false);
            }
            else
            {
                if (panelCanvasGroup != null)
                {
                    panelCanvasGroup.alpha = 0f;
                    panelCanvasGroup.interactable = false;
                    panelCanvasGroup.blocksRaycasts = false;
                }
            }

            currentToSidePos = false;
        }
    }
}
