using System.Collections;
using UnityEngine;

public class MailboxArrow : MonoBehaviour
{
    [Header("UI Arrow")]
    [SerializeField] private RectTransform arrow;   // šipka na canvasu
    [SerializeField] private float amplitude = 10f; // jak vysoko pùjde nahoru/dolu
    [SerializeField] private float speed = 2f;      // rychlost oscilace

    [Header("Player Tracking")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float rotationSmooth = 5f; // plynulost rotace

    private Coroutine animRoutine;
    private Vector2 startPos;
    private Quaternion startRotation;
    private Transform player;

    private void Awake()
    {
        startPos = arrow.anchoredPosition;
        startRotation = arrow.localRotation;
        arrow.gameObject.SetActive(false);

        // Pokusíme se hráèe najít hned pøi startu
        var go = GameObject.FindWithTag(playerTag);
        if (go != null) player = go.transform;
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
        arrow.localRotation = startRotation;
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

            // pokud hráè není nalezen, zkusíme ho najít
            if (player == null)
            {
                var go = GameObject.FindWithTag(playerTag);
                if (go != null) player = go.transform;
            }

            // aktualizujeme rotaci po ose Y podle pozice hráèe v prostoru
            if (player != null)
            {
                // vektor od mailboxu k hráèi (svìtové souøadnice)
                Vector3 dir = player.position - transform.position;

                // získáme yaw úhel (rotace kolem Y)
                float targetY = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

                // plynulé pøiblížení k cílové rotaci
                Quaternion targetRot = Quaternion.Euler(0f, targetY, 0f);
                arrow.localRotation = Quaternion.Slerp(arrow.localRotation, targetRot, Time.deltaTime * rotationSmooth);
            }

            yield return null;
        }
    }
}
