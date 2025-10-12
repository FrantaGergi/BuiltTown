using System.Collections;
using Unity.AppUI.UI;
using UnityEngine;

public class HitShakeEffect : MonoBehaviour
{

    [Header("Chvìní")]
    public float shakeIntensity = 3f;   // úhel vychýlení
    public float shakeSpeed = 12f;      // rychlost pohybu
    public float shakeDuration = 0.3f;  // jak dlouho trvá efekt

    [Header("Micro-scale")]
    public float scaleAmount = 0.05f;   // o kolik se strom „stlaèí“
    public float scaleSpeed = 12f;      // rychlost návratu

    private Quaternion originalRot;
    private Vector3 originalScale;
    private bool shaking = false;

    [Header("Icons")]
    [SerializeField] private float radius = 0.4f;       // kolem kmene
    [SerializeField] private float hopHeight = 1.5f;    // jak vysoko vyskoèí
    [SerializeField] private float hopDistance = 1.5f;  // jak daleko od kmene
    [SerializeField] private float hopTime = 0.6f;      // délka jednoho "hopu"
    [SerializeField] private float idleTime = 30f;     // jak dlouho zùstane na zemi pøed zmizením
    [SerializeField] private LayerMask groundMask;      // pro raycast na zem

    void Start()
    {
        originalRot = transform.rotation;
        originalScale = transform.localScale;
    }

    public void Hit()
    {
        if (!shaking) StartCoroutine(DoShake());
    }


    private IEnumerator DoShake()
    {
        shaking = true;
        float time = 0f;

        while (time < shakeDuration)
        {
            // sinusové vychýlení kmene
            float angle = Mathf.Sin(time * shakeSpeed) * shakeIntensity;
            transform.rotation = originalRot * Quaternion.Euler(0, 0, angle);

            // squash & stretch na scale
            float squash = 1f + Mathf.Sin(time * scaleSpeed) * scaleAmount;
            transform.localScale = new Vector3(originalScale.x, originalScale.y * squash, originalScale.z);

            time += Time.deltaTime;
            yield return null;
        }

        // vrátit zpìt do pùvodního stavu
        transform.rotation = originalRot;
        transform.localScale = originalScale;
        shaking = false;
    }
}
