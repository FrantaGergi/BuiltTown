using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ToolIconTransition : MonoBehaviour
{
    [SerializeField] private Image secondaryToolIcon;
    [SerializeField] private Image primaryToolIcon;

    [SerializeField] private float transitionSpeed = 4f;
    [SerializeField] private float distance = 100f;

    private bool showingPrimary = true;
    private bool isTransitioning = false;

    public void SwitchTool(bool toPrimary)
    {
        if (isTransitioning || toPrimary == showingPrimary) return;
        StartCoroutine(AnimateTransition(toPrimary));
    }

    private IEnumerator AnimateTransition(bool toPrimary)
    {
        isTransitioning = true;

        Image fromIcon = showingPrimary ? secondaryToolIcon : primaryToolIcon;
        Image toIcon = showingPrimary ? primaryToolIcon : secondaryToolIcon;

        Vector3 fromStart = fromIcon.rectTransform.localPosition;
        Vector3 fromEnd = fromStart + Vector3.up * distance;

        Vector3 toStart = toIcon.rectTransform.localPosition - Vector3.up * distance;
        Vector3 toEnd = toIcon.rectTransform.localPosition;

        toIcon.gameObject.SetActive(true);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * transitionSpeed;
            float p = Mathf.SmoothStep(0, 1, t);

            fromIcon.rectTransform.localPosition = Vector3.Lerp(fromStart, fromEnd, p);
            fromIcon.color = new Color(1, 1, 1, 1 - p);

            toIcon.rectTransform.localPosition = Vector3.Lerp(toStart, toEnd, p);
            toIcon.color = new Color(1, 1, 1, p);

            yield return null;
        }

        fromIcon.gameObject.SetActive(false);
        showingPrimary = toPrimary;
        isTransitioning = false;
    }
}
