using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static MinimapIconManager;
using static UnityEngine.GraphicsBuffer;

public class UIMinimapController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private int imageChildIndex = 2; // index dítìte s Image
    [SerializeField] private Color inactiveColor = Color.white;
    [SerializeField] private Color activeColor = Color.green;
    [SerializeField] private float toggleXFalse = -45f;
    [SerializeField] private float toggleXTrue = 45f;
    [SerializeField] private float lerpDuration = 0.3f;

    [Header("Button Mappings")]
    public ButtonIconMapping[] buttonIconMappings;

    [Serializable]
    public struct ButtonIconMapping
    {
        public Button button;
        public MinimapIconType iconType;
    }

    private Dictionary<Button, Image> buttonImages = new Dictionary<Button, Image>();
    private Dictionary<Button, bool> buttonStates = new Dictionary<Button, bool>();

    void Start()
    {
        foreach (var mapping in buttonIconMappings)
        {
            Button b = mapping.button;
            if (b == null) continue;

            // získáme dítì s Image podle indexu
            Image img = b.transform.GetChild(imageChildIndex).GetComponent<Image>();
            if (img == null) continue;

            buttonImages[b] = img;

            // nastavení poèáteèní pozice a barvy
            img.rectTransform.localPosition = new Vector3(toggleXTrue, img.rectTransform.localPosition.y, img.rectTransform.localPosition.z);
            img.color = activeColor;

            // poèáteèní stav tlaèítka
            buttonStates[b] = true;

            // registrace listeneru s lokální kopií promìnné
            Button localButton = b;
            b.onClick.AddListener(() => ToggleButton(localButton));

        }
    }

    void ToggleButton(Button b)
    {
        if (!buttonImages.TryGetValue(b, out Image img) || img == null) return;

        // pøepnutí stavu tlaèítka
        buttonStates[b] = !buttonStates[b];


        float targetX = buttonStates[b] ? toggleXTrue : toggleXFalse;
        Color targetColor = buttonStates[b] ? activeColor : inactiveColor;

        StartCoroutine(MoveAndColor(img, targetX, targetColor));



        // Získání typu ikony z ButtonIconMapping
        MinimapIconType type = MinimapIconType.None; // default
        foreach (var mapping in buttonIconMappings)
        {
            if (mapping.button == b)
            {
                type = mapping.iconType;
                break;
            }
        }

        // Zavolání MinimapIconManager s novou hodnotou
        MinimapIconManager.Instance.SetGroupVisible(type, buttonStates[b]);

    }

    IEnumerator MoveAndColor(Image img, float targetX, Color targetColor)
    {
        if (img == null) yield break;

        float elapsed = 0f;
        RectTransform rect = img.rectTransform;
        Vector3 startPos = rect.localPosition;
        Vector3 endPos = new Vector3(targetX, startPos.y, startPos.z);
        Color startColor = img.color;

        while (elapsed < lerpDuration)
        {
            if (img == null) yield break;

            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / lerpDuration);

            rect.localPosition = Vector3.Lerp(startPos, endPos, t);
            img.color = Color.Lerp(startColor, targetColor, t);

            yield return null;
        }

        if (img != null)
        {
            rect.localPosition = endPos;
            img.color = targetColor;
        }
    }

}
