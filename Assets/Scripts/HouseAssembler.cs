using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseAssembler
{
    [System.Serializable]
    private class PartData
    {
        public Transform transform;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
        public Collider collider; // uložíme si collider (pokud existuje)
    }

    private List<PartData> parts = new List<PartData>();

    [Header("Nastavení efektu")]
    [SerializeField] private float assembleSpeed = 3f; // rychlost návratu èástí
    [SerializeField] private float randomOffset = 5f;  // jak daleko budou èásti rozházené pøi zaèátku
    private MonoBehaviour runner;

    public HouseAssembler(MonoBehaviour runner)
    {
        this.runner = runner;
    }

    /// <summary>
    /// Inicializuje dùm a rozhází jeho èásti náhodnì okolo.
    /// </summary>
    private void Scatter(GameObject house)
    {
        parts.Clear();

        foreach (Transform child in house.transform)
        {
            PartData data = new PartData();
            data.transform = child;
            data.originalPosition = child.localPosition;
            data.originalRotation = child.localRotation;
            data.collider = child.GetComponent<Collider>();

            // vypneme collider, pokud existuje
            if (data.collider != null)
                data.collider.enabled = false;

            // rozházíme èást
            Vector3 offset = Random.insideUnitSphere * randomOffset;
            child.localPosition += offset;
            child.localRotation = Random.rotation;

            parts.Add(data);
        }
    }

    /// <summary>
    /// Spustí animaci, kdy se všechny èásti vrátí do pùvodní polohy a rotace.
    /// </summary>
    public void Assemble(GameObject house)
    {
        Scatter(house);
        runner.StartCoroutine(AssembleRoutine());
    }

    private IEnumerator AssembleRoutine()
    {
        bool assembling = true;
        while (assembling)
        {
            assembling = false;

            foreach (var part in parts)
            {
                if (part.transform == null)
                    continue;

                part.transform.localPosition = Vector3.Lerp(
                    part.transform.localPosition,
                    part.originalPosition,
                    Time.deltaTime * assembleSpeed
                );

                part.transform.localRotation = Quaternion.Slerp(
                    part.transform.localRotation,
                    part.originalRotation,
                    Time.deltaTime * assembleSpeed
                );

                // dokud nejsme blízko cíli, pokraèuj
                if (Vector3.Distance(part.transform.localPosition, part.originalPosition) > 0.01f)
                    assembling = true;
            }

            yield return null;
        }

        foreach (var part in parts)
        {
            if (part.transform == null)
                continue;

            part.transform.localPosition = part.originalPosition;
            part.transform.localRotation = part.originalRotation;

            if (part.collider != null)
                part.collider.enabled = true;
        }
    }

}
