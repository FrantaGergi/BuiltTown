using System.Collections;
using TMPro;
using UnityEngine;


public class BuildingCanvV2 : MonoBehaviour
{
    [SerializeField] private RectTransform progressBar;
    [SerializeField] private RectTransform progreesParent;
    [SerializeField] private TextMeshProUGUI progressText;

    [SerializeField] private RectTransform parentPanel;
    [SerializeField] private ItemType ItemType;

    [SerializeField] private CircularClickGame CircularClickGame;


    private void Start()
    {
        SetProgressBar(ItemType, 0f);

    }


    public void SetProgressBar(ItemType itemType, float value01)
    {
        // Omez hodnotu na rozsah 0–1
        value01 = Mathf.Clamp01(value01);
        float maxWidth = progreesParent.rect.width;

        Vector2 size = progressBar.sizeDelta;
        size.x = maxWidth * value01;

        progressBar.sizeDelta = size;
    }

    public void SetProgressBarSmooth(float targetValue01, float duration = 0.5f)
    {
        StartCoroutine(AnimateProgressBar(targetValue01, duration));
    }

    private IEnumerator AnimateProgressBar(float targetValue01, float duration)
    {
        float startValue = progressBar.sizeDelta.x;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            // Interpoluj šíøku mezi startovní a cílovou hodnotou
            float newValue = Mathf.Lerp(startValue, progreesParent.rect.width * targetValue01, timeElapsed / duration);
            Vector2 size = progressBar.sizeDelta;
            size.x = newValue;
            progressBar.sizeDelta = size;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ujisti se, že na konci je pøesná cílová hodnota
        Vector2 finalSize = progressBar.sizeDelta;
        finalSize.x = progreesParent.rect.width * targetValue01;
        progressBar.sizeDelta = finalSize;
    }
    public void SetText(int current, int max)
    {  
        progressText.text = $"{current}/{max}";
    }

}

