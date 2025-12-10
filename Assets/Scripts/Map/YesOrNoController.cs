using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class YesOrNoController : MonoBehaviour
{
    [Header("References ->")]
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    public bool YesOrNoPanelEnabled = false;

    [Header("Animation")]
    [SerializeField, Tooltip("Délka animace otevøení / zavøení v sekundách")] private float animationDuration = 0.18f;
    [SerializeField, Tooltip("Easing køivka pro animaci (0..1)")] private AnimationCurve ease = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    private Coroutine showHideCoroutine;

    private void Start()
    {
        // Inicializace skrytého panelu a scale = 0
        if (transform is RectTransform)
        {
            transform.localScale = Vector3.zero;
        }
        HideImmediate();
    }

    public void Show(string content, System.Action yesAction)
    {
        contentText.text = content;
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() =>
        {
            yesAction?.Invoke();
            Hide();
        });
        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(Hide);

        // Deaktivujeme okamžitou interakci tlaèítek — zabráníme "propagaci" pùvodního kliknutí, které otevøelo panel.
        yesButton.interactable = false;
        noButton.interactable = false;

        // Aktivace panelu
        gameObject.SetActive(true);
        YesOrNoPanelEnabled = true;

        // Vyèistíme vybraný UI element, aby se nepøenášelo selektování / starý input
        EventSystem.current?.SetSelectedGameObject(null);

        // Spustíme animaci otevøení (scale 0 -> 1). Po skonèení povolíme tlaèítka.
        RestartShowHideCoroutine(ShowSequence(true));
    }

    public void Hide()
    {
        // Spustit animaci zavøení (scale 1 -> 0) a poté deaktivovat panel.
        RestartShowHideCoroutine(ShowSequence(false));
    }

    private void HideImmediate()
    {
        // okamžitì skryje bez animace (použito ve Start)
        StopShowHideCoroutine();
        gameObject.SetActive(false);
        YesOrNoPanelEnabled = false;
        if (transform != null) transform.localScale = Vector3.zero;
    }

    private void RestartShowHideCoroutine(IEnumerator routine)
    {
        StopShowHideCoroutine();
        showHideCoroutine = StartCoroutine(routine);
    }

    private void StopShowHideCoroutine()
    {
        if (showHideCoroutine != null)
        {
            StopCoroutine(showHideCoroutine);
            showHideCoroutine = null;
        }
    }

    private IEnumerator ShowSequence(bool opening)
    {
        float t = 0f;
        Vector3 start = opening ? Vector3.zero : transform.localScale;
        Vector3 target = opening ? Vector3.one : Vector3.zero;

        // Pokud zaèínáme z jiné hodnoty scale, bereme ji jako start (robustní vùèi opakovanému volání)
        if (transform != null)
            start = transform.localScale;

        // Pokud animationDuration je 0, pøepneme okamžitì
        if (Mathf.Approximately(animationDuration, 0f))
        {
            transform.localScale = target;
        }
        else
        {
            while (t < animationDuration)
            {
                t += Time.deltaTime;
                float norm = Mathf.Clamp01(t / animationDuration);
                float eased = ease != null ? ease.Evaluate(norm) : Mathf.SmoothStep(0f, 1f, norm);
                if (transform != null)
                    transform.localScale = Vector3.LerpUnclamped(start, target, eased);
                yield return null;
            }
            if (transform != null)
                transform.localScale = target;
        }

        // Po dokonèení animace:
        if (opening)
        {
            // povolit tlaèítka až nyní
            yesButton.interactable = true;
            noButton.interactable = true;
            EventSystem.current?.SetSelectedGameObject(yesButton.gameObject);
        }
        else
        {
            // zavøeno: deaktivovat a resetovat stav
            gameObject.SetActive(false);
            YesOrNoPanelEnabled = false;
            // zajistit že scale je 0
            if (transform != null) transform.localScale = Vector3.zero;
        }

        showHideCoroutine = null;
    }
}
