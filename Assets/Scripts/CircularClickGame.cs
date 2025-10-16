using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CircularClickGame : MonoBehaviour
{
    [Header("Odkazy")]
    [SerializeField] private Transform[] logs; // Logy (3x)
    [SerializeField] private Transform pointer; // Bílé koleèko
    [SerializeField] private Transform centerIcon; // Ikona uprostøed A
    [SerializeField] private Transform centerIconB; // Ikona uprostøed B
    [SerializeField] private Transform iconTarget; // Kam má odletìt staré centrum pøi pøepnutí

    [Header("Nastavení")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float hitAngleTolerance = 10f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color failColor = Color.red;
    [SerializeField] private float highlightDuration = 0.2f;
    [SerializeField] private float logJumpDuration = 0.3f;
    [SerializeField] private float scaleIncrement = 0.25f; // o kolik se zvìtší støed po každém hitu
    [SerializeField] private float flyOutDuration = 0.3f; // délka animace odletu starého centra

    [Header("Efekty")]
    [SerializeField] private ParticleSystem poofEffect;

    private float startRotation = -135.268f;
    private float endRotation = 224.732f;
    private float currentRotation;
    private Image pointerImage;
    private Vector3 originalCenterScale;
    private Coroutine pulseCoroutine;
    private float accumulatedScaleMultiplier = 1f;

    private Vector3 originalCenterPosA;
    private Vector3 originalCenterPosB;

    // Pøepínaè mezi centry
    private bool usingCenterA = true;

    private void Start()
    {
        currentRotation = startRotation;
        pointerImage = pointer.GetComponent<Image>();
        pointerImage.color = normalColor;

        originalCenterScale = centerIcon.localScale;

        // Na zaèátku zapni A a vypni B
        centerIcon.gameObject.SetActive(true);
        centerIconB.gameObject.SetActive(false);

        originalCenterPosA = centerIcon.localPosition;
        originalCenterPosB = centerIconB.localPosition;


        RandomizeAndEnableLogs();
    }

    private void Update()
    {
        RotateCircle();
    }

    private void RotateCircle()
    {
        currentRotation += rotationSpeed * Time.deltaTime;
        if (currentRotation >= endRotation)
        {
            currentRotation = startRotation;
            FullRotationCompleted();
        }

        pointer.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }

    private void FullRotationCompleted()
    {
        // Pøi každé otoèce pøepni centrum
        SwitchCenterIcons();

        // Reset scale a logù pro aktivní centrum
        accumulatedScaleMultiplier = 1f;
        GetActiveCenter().localScale = originalCenterScale;
        RandomizeAndEnableLogs();
    }

    /// <summary>
    /// Pøepne aktivní center mezi centerIcon a centerIconB
    /// </summary>
    private void SwitchCenterIcons()
    {
        // zjisti staré a nové centrum
        Transform oldCenter = GetActiveCenter();
        usingCenterA = !usingCenterA; // pøepnutí hodnoty
        Transform newCenter = GetActiveCenter();

        // zapni nové centrum, resetuj pozici a scale na 0
        newCenter.gameObject.SetActive(true);
        newCenter.localPosition = usingCenterA ? originalCenterPosA : originalCenterPosB;
        newCenter.localScale = Vector3.zero;

        // spust animaci zvìtšení nového centra
        StartCoroutine(ScaleUpCenter(newCenter, originalCenterScale, 0.5f));

        // spust animaci odletu starého
        StartCoroutine(FlyOutAndDisable(oldCenter));
    }

    /// <summary>
    /// Staré centrum odletí k cíli a zmizí, pak se vypne a resetuje pozici
    /// </summary>
    private IEnumerator FlyOutAndDisable(Transform center)
    {
        Vector3 startPos = center.position;
        Vector3 targetPos = iconTarget.position;
        Vector3 startScale = center.localScale;
        Vector3 endScale = Vector3.zero;

        float elapsed = 0f;
        while (elapsed < flyOutDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / flyOutDuration);
            center.position = Vector3.Lerp(startPos, targetPos, t);
            center.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // dokonèíme pøesnì
        center.position = startPos; // reset na pùvodní pozici
        center.localScale = originalCenterScale;
        center.gameObject.SetActive(false);
    }

    /// <summary>
    /// Vrací právì aktivní center transform
    /// </summary>
    private Transform GetActiveCenter()
    {
        return usingCenterA ? centerIcon : centerIconB;
    }

    private void RandomizeAndEnableLogs()
    {
        foreach (var l in logs)
        {
            l.gameObject.SetActive(true);
            l.localScale = Vector3.one;
            l.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }
    }

    private IEnumerator ScaleUpCenter(Transform center, Vector3 targetScale, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            center.localScale = Vector3.Lerp(Vector3.zero, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        center.localScale = targetScale;
    }

    public void OnHit(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            TryHit();
    }

    private void TryHit()
    {
        bool hit = false;

        foreach (var log in logs)
        {
            float angleDiff = Mathf.DeltaAngle(pointer.localEulerAngles.z, log.localEulerAngles.z);
            if (Mathf.Abs(angleDiff) <= hitAngleTolerance)
            {
                StartCoroutine(HitLog(log));
                hit = true;
                break;
            }
        }

        if (!hit)
            StartCoroutine(FailEffect());
    }

    private IEnumerator HitLog(Transform log)
    {
        if (poofEffect != null)
        {
            poofEffect.transform.position = pointer.position;
            poofEffect.Play();
        }

        yield return StartCoroutine(LogJumpToCenter(log));
        StartCoroutine(HighlightPointer(successColor));

        if (pulseCoroutine != null)
            StopCoroutine(pulseCoroutine);
        pulseCoroutine = StartCoroutine(PulseCenterIcon());
    }

    private IEnumerator LogJumpToCenter(Transform log)
    {
        Transform activeCenter = GetActiveCenter();

        Vector3 startPos = log.position;
        Vector3 targetPos = activeCenter.position;
        float elapsed = 0f;

        Vector3 startScale = log.localScale;
        Vector3 endScale = Vector3.zero;

        while (elapsed < logJumpDuration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / logJumpDuration);
            log.position = Vector3.Lerp(startPos, targetPos, t);
            log.localScale = Vector3.Lerp(startScale, endScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        log.gameObject.SetActive(false);
    }

    private IEnumerator HighlightPointer(Color color)
    {
        pointerImage.color = color;
        yield return new WaitForSeconds(highlightDuration);
        pointerImage.color = normalColor;
    }

    private IEnumerator PulseCenterIcon()
    {
        Transform activeCenter = GetActiveCenter();

        Vector3 currentScale = activeCenter.localScale;
        accumulatedScaleMultiplier += scaleIncrement;

        Vector3 targetScale = originalCenterScale * accumulatedScaleMultiplier * 1.5f;
        Vector3 finalScale = originalCenterScale * accumulatedScaleMultiplier;

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            activeCenter.localScale = Vector3.Lerp(currentScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            activeCenter.localScale = Vector3.Lerp(targetScale, finalScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        activeCenter.localScale = finalScale;
    }

    private IEnumerator FailEffect()
    {
        pointerImage.color = failColor;

        Vector3 originalPos = pointer.position;
        float elapsed = 0f;
        float duration = 0.2f;

        while (elapsed < duration)
        {
            pointer.position = originalPos + Random.insideUnitSphere * 0.05f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        pointer.position = originalPos;
        pointerImage.color = normalColor;
    }
}
