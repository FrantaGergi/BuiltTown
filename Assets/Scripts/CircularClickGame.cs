using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CircularClickGame : MonoBehaviour
{
    [Header("Odkazy")]
    [SerializeField] private Transform[] logs; // Logy (3x)
    [SerializeField] private Transform pointer; // Bílé koleèko
    [SerializeField] private Transform centerIcon; // Ikona uprostøed

    [Header("Nastavení")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float hitAngleTolerance = 10f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color failColor = Color.red;
    [SerializeField] private float highlightDuration = 0.2f;
    [SerializeField] private float logJumpDuration = 0.3f;

    [Header("Efekty")]
    [SerializeField] private ParticleSystem poofEffect;

    private float startRotation = -135.268f;
    private float endRotation = 224.732f;
    private float currentRotation;
    private Image pointerImage;
    private Transform originalSizeCenterIcon;
    private Transform currentSizeCenterIcon;

    private void Start()
    {
        currentRotation = startRotation;
        pointerImage = pointer.GetComponent<Image>();
        pointerImage.color = normalColor;
        originalSizeCenterIcon = centerIcon;

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
        // pokaždé, když se kruh otoèí celý -> obnov všechny logy
        RandomizeAndEnableLogs();
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
        // Poof efekt
        if (poofEffect != null)
        {
            poofEffect.transform.position = pointer.position;
            poofEffect.Play();
        }

        // Log "skoèí" do støedu
        yield return StartCoroutine(LogJumpToCenter(log));

        // Pointer barva
        yield return StartCoroutine(HighlightPointer(successColor));

        // Puls ikony ve støedu
        yield return StartCoroutine(PulseCenterIcon());
    }

    private IEnumerator LogJumpToCenter(Transform log)
    {
        Vector3 startPos = log.position;
        Vector3 targetPos = centerIcon.position;
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
        Vector3 originalScale = currentSizeCenterIcon.localScale;
        Vector3 targetScale = currentSizeCenterIcon.localScale * 1.3f; // zvìtšení o 30 %
        Vector3 smallerScale = Vector3.Lerp(targetScale, originalScale, 1f / 3f);
        // tedy zmenšení o 1/3 zvìtšení, napø. 1.3x -> 1.1x

        float duration = 0.15f;
        float elapsed = 0f;

        // Zvìtšení
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            centerIcon.localScale = Vector3.Lerp(originalScale, targetScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Zmenšení zpìt (jen èásteènì)
        elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            centerIcon.localScale = Vector3.Lerp(targetScale, smallerScale, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Návrat na pùvodní velikost
        centerIcon.localScale = smallerScale;
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
