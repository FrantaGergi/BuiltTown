using System.Collections;
using UnityEngine;

public class MailboxArrow : MonoBehaviour
{
    [Header("UI Arrow")]
    [SerializeField] private RectTransform arrow;   // šipka na canvasu
    [SerializeField] private float amplitude = 10f; // jak vysoko pùjde nahoru/dolu
    [SerializeField] private float speed = 2f;      // rychlost oscilace

    private Coroutine animRoutine;
    private Vector2 startPos;

    private void Awake()
    {
        startPos = arrow.anchoredPosition;
        arrow.gameObject.SetActive(false);
    }

    private void Start()
    {
        ShowArrow();
    }

    public void ShowArrow()
    {
        arrow.gameObject.SetActive(true);

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(ArrowFloat());
    }

    public void HideArrow()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        arrow.anchoredPosition = startPos;
        arrow.gameObject.SetActive(false);
    }

    private IEnumerator ArrowFloat()
    {
        float t = 0f;

        while (true)
        {
            t += Time.deltaTime * speed;

            float offset = Mathf.Sin(t) * amplitude;

            arrow.anchoredPosition = startPos + new Vector2(0, offset);

            yield return null;
        }
    }
}
