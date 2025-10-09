using System.Collections;
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
    public void SpawnIcons(int count, GameObject prefab, float force = 3f)
    {
   
        Vector3 origin = transform.position + Vector3.up * 1.5f; // místo kde to "vyletí"

        for (int i = 0; i < count; i++)
        {
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = Mathf.Abs(randomDir.y); // aby neletìly pod zem
            Vector3 spawnPos = origin + Random.insideUnitSphere * 0.3f;

            GameObject icon = Instantiate(prefab, spawnPos, Quaternion.identity);
            IconToSpawn wood = icon.GetComponent<IconToSpawn>();
            wood.Launch(randomDir * force, Random.Range(1.5f, 3f));
        }
    }
}
