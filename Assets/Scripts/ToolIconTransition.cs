using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ToolIconTransition : MonoBehaviour
{
    [Header("Icons with Borders")]
    [SerializeField] private Image primaryBorder;
    [SerializeField] private Image middleBorder;
    [SerializeField] private Image secondaryBorder;

    [Header("Transition Settings")]
    [SerializeField] private float fadeSpeed = 6f;

    private Image currentBorder;
    private Image targetBorder;
    private Coroutine fadeRoutine;

    private Dictionary<Image, float> initialAlphas = new Dictionary<Image, float>();

    private void Awake()
    {
        // Inicializace použijeme v Awake, aby byla pøipravená døív než Start jiných skriptù
        CacheInitialAlpha(primaryBorder);
        CacheInitialAlpha(middleBorder);
        CacheInitialAlpha(secondaryBorder);

        // Vypneme všechny bordery ihned
        SetAlpha(primaryBorder, 0f);
        SetAlpha(middleBorder, 0f);
        SetAlpha(secondaryBorder, 0f);
    }

    private void Start()
    {
        // nic zvláštního už dìlat nemusíme, ale Start necháme pro kompatibilitu
    }

    private void CacheInitialAlpha(Image img)
    {
        if (img != null && !initialAlphas.ContainsKey(img))
            initialAlphas.Add(img, img.color.a);
    }

    public void SwitchTool(bool toPrimary)
    {
        if (toPrimary)
            SwitchTo(primaryBorder);
        else
            SwitchTo(secondaryBorder);
    }

    // SwitchToNone nyní pøepne na middleBorder (pokud chceš støední border vidìt jako "none")
    public void SwitchToNone()
    {
        SwitchTo(middleBorder);
    }

    private void SwitchTo(Image newBorder)
    {

        if (currentBorder == newBorder) return;

        targetBorder = newBorder;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeTransition());
    }

    private IEnumerator FadeTransition()
    {
        Image previous = currentBorder;
        currentBorder = targetBorder;

        float targetAlpha = (currentBorder != null && initialAlphas.ContainsKey(currentBorder))
            ? initialAlphas[currentBorder]
            : 1f;

        while (true)
        {
            bool donePrev = true;
            bool doneCurr = true;

            if (previous)
            {
                float a = Mathf.Lerp(previous.color.a, 0f, Time.deltaTime * fadeSpeed);
                SetAlpha(previous, a);
                donePrev = Mathf.Abs(a - 0f) < 0.01f;
            }

            if (currentBorder)
            {
                float a = Mathf.Lerp(currentBorder.color.a, targetAlpha, Time.deltaTime * fadeSpeed);
                SetAlpha(currentBorder, a);
                doneCurr = Mathf.Abs(a - targetAlpha) < 0.01f;
            }

            if (donePrev && doneCurr)
                break;

            yield return null;
        }

        if (previous) SetAlpha(previous, 0f);
        if (currentBorder && initialAlphas.ContainsKey(currentBorder))
            SetAlpha(currentBorder, initialAlphas[currentBorder]);

        fadeRoutine = null;
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (!img) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
}
