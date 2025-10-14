using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static InteractManager;

public class CircularClickGame : MonoBehaviour
{
    [Header("Odkazy")]
    [SerializeField] private Transform[] logs; // Logy (3x)
    [SerializeField] private Transform Pointer; // Bílé koleèko
    [SerializeField] private Transform centerIcon; // Ikona uprostøed

    [Header("Nastavení")]
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float hitAngleTolerance = 10f;
    [SerializeField] private Color successColor = Color.green;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color failColor = Color.red;
    [SerializeField] private float highlightDuration = 0.2f;

    [Header("Efekty")]
    [SerializeField] private ParticleSystem poofEffect;

    private float startRotation = -135.268f;
    private float endRotation = 224.732f;
    private float currentRotation; // výchozí úhel

    private void Start()
    {
        currentRotation = startRotation;
        Pointer.GetComponent<Image>().color = normalColor;
    }

    private void Update()
    {
        RotateCircle();
    }

    private void FullRotationCompleted()
    {
        // Reset logù (znovu zapnout + nová náhodná rotace)
        foreach (var l in logs)
        {
            l.gameObject.SetActive(true);
            l.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }
    }

    private void RotateCircle()
    {
        currentRotation += rotationSpeed * Time.deltaTime;
        if (currentRotation >= endRotation)
        {
            currentRotation = startRotation;
            FullRotationCompleted();
        }
        Pointer.localRotation = Quaternion.Euler(0, 0, currentRotation);
    }

    public void OnHit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryHit();
    }
    public void TryHit()
    {
            bool hit = false;

            foreach (var log in logs)
            {
                float angleDiff = Mathf.DeltaAngle(Pointer.localEulerAngles.z, log.localEulerAngles.z);
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
        log.gameObject.SetActive(false);

        // Zvýraznìní støedu
        StartCoroutine(HighlightCenter(successColor));

        // Poof efekt
        if (poofEffect != null)
        {
            poofEffect.transform.position = Pointer.position;
            poofEffect.Play();
        }

        // Animace støedu (zvìtšení + zatoèení)
        Vector3 originalScale = Pointer.localScale;
        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            float scale = Mathf.Lerp(1f, 1.2f, elapsed / duration);
            Pointer.localScale = originalScale * scale;
            Pointer.Rotate(Vector3.forward * 360f * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }
        Pointer.localScale = originalScale;


    }

    private IEnumerator HighlightCenter(Color color)
    {
        Image sr = Pointer.GetComponent<Image>();
        Color original = sr.color;
        sr.color = color;
        yield return new WaitForSeconds(highlightDuration);
        sr.color = original;
    }

    private IEnumerator FailEffect()
    {
        Image sr = Pointer.GetComponent<Image>();
        Color original = sr.color;
        sr.color = failColor;

        Vector3 originalPos = Pointer.position;
        float elapsed = 0f;
        float duration = 0.2f;
        while (elapsed < duration)
        {
            Pointer.position = originalPos + Random.insideUnitSphere * 0.05f;
            elapsed += Time.deltaTime;
            yield return null;
        }

        Pointer.position = originalPos;
        sr.color = original;
    }
}
