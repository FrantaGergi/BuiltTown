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
    private Vector3 startLocalPos;
    private Quaternion startRotation;
    private Transform player;

    private void Awake()
    {
        startLocalPos = arrow.localPosition;
        startRotation = arrow.localRotation;
        arrow.gameObject.SetActive(false);

        // Pokusíme se hráèe najít hned pøi startu
        var go = GameObject.FindWithTag(playerTag);
        if (go != null) player = go.transform;
    }

    private void OnEnable()
    {
        ShowArrow();
    }

    public void ShowArrow()
    {
        arrow.gameObject.SetActive(true);
        Debug.Log("Showing Arrow");

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(ArrowFloat());
    }

    public void HideArrow()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        arrow.localPosition = startLocalPos;
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

            arrow.localPosition = startLocalPos + new Vector3(0f, offset, 0f);

            // pokud hráè není nalezen, zkusíme ho najít
            if (player == null)
            {
                var go = GameObject.FindWithTag(playerTag);
                if (go != null) player = go.transform;
            }

            // aktualizujeme rotaci po ose Y podle pozice hráèe v prostoru
            if (player != null)
            {
                // vektor od mailboxu k hráèi (svìtové souøadnice) a ignorujeme výšku
                Vector3 dir = player.position - transform.position;
                dir.y = 0f;

                if (dir.sqrMagnitude > 0.0001f)
                {
                    // úhel mezi "forward" mailboxu a smìrem na hráèe (v°)
                    float angle = Vector3.SignedAngle(transform.forward, dir.normalized, Vector3.up);

                    // cílová lokální rotace: natoèíme šipku o tento úhel relativnì k její startovní lokální rotaci
                    Quaternion targetLocalRot = Quaternion.Euler(0f, angle, 0f) * startRotation;

                    // plynulé pøiblížení k cílové lokální rotaci
                    arrow.localRotation = Quaternion.Slerp(arrow.localRotation, targetLocalRot, Time.deltaTime * rotationSmooth);
                }
            }

            yield return null;
        }
    }
}