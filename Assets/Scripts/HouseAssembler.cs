using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseAssembler : MonoBehaviour
{
    [System.Serializable]
    private class PartData
    {
        public Transform transform;
        public Vector3 originalPosition;
        public Quaternion originalRotation;
    }

    private List<PartData> parts = new List<PartData>();

    [Header("Nastavení efektu")]
    [SerializeField] private float assembleSpeed = 3f; // rychlost návratu èástí
    [SerializeField] private float randomOffset = 5f;  // jak daleko budou èásti rozházené pøi zaèátku

    /// <summary>
    /// Inicializuje dùm a rozhází jeho èásti náhodnì okolo.
    /// </summary>
    public void Scatter(GameObject house)
    {
        parts.Clear();

        foreach (Transform child in house.transform)
        {
            PartData data = new PartData();
            data.transform = child;
            data.originalPosition = child.localPosition;
            data.originalRotation = child.localRotation;

            // rozhodíme ho náhodnì kolem
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
        StopAllCoroutines();
        StartCoroutine(AssembleRoutine());
    }

    private IEnumerator AssembleRoutine()
    {
        bool assembling = true;
        while (assembling)
        {
            assembling = false;

            foreach (var part in parts)
            {
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
    }
}
