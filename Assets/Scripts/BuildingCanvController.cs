using FGUIStarter;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

public class BuildingCanvController : MonoBehaviour
{
    [SerializeField, Header("Panels representing a max size of progrees bars")]
    private RectTransform panel1;

    [SerializeField]
    private RectTransform progressBar1, progressBar2, progressBar3;
    [SerializeField]
    private CustomButton button1, button2, button3;
    [SerializeField]
    private TextMeshProUGUI text1, text2, text3;

    [SerializeField]
    private float simulatedPressDuration = 0.1f;

    [Header("colors of buttons")]
    [SerializeField] private Sprite GrayButton;
    [SerializeField] private Sprite GreenButton;
    [SerializeField] private Sprite PressedGrayButton;
    [SerializeField] private Sprite PressedGreenButton;
    [SerializeField] private Sprite FinishedButton;

    private Image bttnImg1, bttnImg2, bttnImg3;

    [Header("use2Source")]
    [SerializeField] private RectTransform PanelFor3Source;
    [SerializeField] private RectTransform PanelFor2Source;
    [SerializeField] private RectTransform HideThirdSourcePanel;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bttnImg1 = button1.GetComponent<Image>();
        bttnImg2 = button2.GetComponent<Image>();
        bttnImg3 = button3.GetComponent<Image>();

        SetProgressBar(ItemType.Wood, 0f);
        SetProgressBar(ItemType.Stone, 0f);
        SetProgressBar(ItemType.Ore, 0f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PressButton(ItemType itemType)
    {
        CustomButton target = null;
        if (itemType == ItemType.Wood)
            target = button1;
        else if (itemType == ItemType.Stone)
            target = button2;
        else if (itemType == ItemType.Ore)
            target = button3;

        if (target == null)
        {
            Debug.LogWarning($"BuildingCanvController: žádné tlaèítko pro {itemType}");
            return;
        }

        // Spustíme simulovaný stisk (vizuál + onClick fallback)
        StartCoroutine(SimulatePress(target, itemType));
    }

    private IEnumerator SimulatePress(CustomButton btn, ItemType itemType)
    {
        if (btn == null) yield break;

        // Pokud máme EventSystem, pošleme pointer down/up pro vizuální efekt a logiku v CustomButton
        if (EventSystem.current != null)
        {
            var evt = new PointerEventData(EventSystem.current);
            // Pointer down -> vizuální isGray stav
            btn.OnPointerDown(evt);
            yield return new WaitForSeconds(simulatedPressDuration);
            // Pointer up -> uvolnìní (a pokud je implementováno správnì, zavolá onClick)
            btn.OnPointerUp(evt);
        }
        else
        {
            // Fallback: rovnou vyvolej onClick, pokud není EventSystem dostupný
            btn.onClick?.Invoke();
            yield return null;
        }
        SetButtonSprite(itemType, true);
    }

    /// <summary>
    /// Nastaví sprite na tlaèítku podle ItemType a stavu (isGray/normal).
    /// </summary>
    
    public void SetAllButtonsGray()
    {
        SetButtonSprite(ItemType.Wood, true);
        SetButtonSprite(ItemType.Stone, true);
        SetButtonSprite(ItemType.Ore, true);
    }
    public void SetButtonToFinished(ItemType itemType)
    {
        Image target = null;
        switch (itemType)
        {
            case ItemType.Wood:
                target = bttnImg1;
                break;
            case ItemType.Stone:
                target = bttnImg2;
                break;
            case ItemType.Ore:
                target = bttnImg3;
                break;
            default:
                Debug.LogWarning($"SetButtonToFinished: Neznámý ItemType {itemType}");
                return;
        }
        if (target == null)
        {
            Debug.LogWarning($"SetButtonToFinished: Nenalezeno tlaèítko pro {itemType}");
            return;
        }
        target.sprite = FinishedButton;
        target.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "";
    }

    public void SetButtonSprite(ItemType itemType, bool isGray = false)
    {
        Image target = null;
        Sprite sprite = null;

        switch (itemType)
        {
            case ItemType.Wood:
                target = bttnImg1;
                sprite = isGray ? GrayButton : GreenButton;
                break;
            case ItemType.Stone:
                target = bttnImg2;
                sprite = isGray ? GrayButton : GreenButton;
                break;
            case ItemType.Ore:
                target = bttnImg3;
                sprite = isGray ? GrayButton : GreenButton;
                break;
            default:
                Debug.LogWarning($"SetButtonSprite: Neznámý ItemType {itemType}");
                return;
        }

        if (target == null)
        {
            Debug.LogWarning($"SetButtonSprite: Nenalezeno tlaèítko pro {itemType}");
            return;
        }
        if(target.sprite == FinishedButton)
        {
            // Pokud je tlaèítko ve stavu "Finished", nemìòme ho
            return;
        }
        target.sprite = sprite;
    }

    public void SetProgressBar(ItemType itemType, float value01)
    {
        // Omez hodnotu na rozsah 0–1
        value01 = Mathf.Clamp01(value01);

        // Zjisti maximální šíøku podle panel1
        float maxWidth = panel1.rect.width;

        // Vyber správný progressBar podle typu
        RectTransform target = null;
        switch (itemType)
        {
            case ItemType.Wood:
                target = progressBar1;
                break;
            case ItemType.Stone:
                target = progressBar2;
                break;
            case ItemType.Ore:
                target = progressBar3;
                break;
            default:
                Debug.LogWarning($"SetProgressBar: Neznámý ItemType {itemType}");
                return;
        }

        if (target == null)
        {
            Debug.LogWarning($"SetProgressBar: Nenalezen progressBar pro {itemType}");
            return;
        }

        // Nastav novou šíøku
        Vector2 size = target.sizeDelta;
        size.x = maxWidth * value01;
        target.sizeDelta = size;
    }

    public void SetProgressBarSmooth(ItemType itemType, float targetValue01, float duration = 0.5f)
    {
        StartCoroutine(AnimateProgressBar(targetValue01, duration, itemType));
    }

    private IEnumerator AnimateProgressBar(float targetValue01, float duration, ItemType itemType)
    {
        // Zjisti cílový progressBar podle typu
        RectTransform target = null;
        switch (itemType)
        {
            case ItemType.Wood:
                target = progressBar1;
                break;
            case ItemType.Stone:
                target = progressBar2;
                break;
            case ItemType.Ore:
                target = progressBar3;
                break;
            default:
                Debug.LogWarning($"AnimateProgressBar: Neznámý ItemType {itemType}");
                yield break;
        }

        if (target == null)
        {
            Debug.LogWarning($"AnimateProgressBar: Nenalezen progressBar pro {itemType}");
            yield break;
        }

        // Aktuální šíøka
        float startValue = target.sizeDelta.x;
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            // Interpoluj šíøku mezi startovní a cílovou hodnotou
            float newValue = Mathf.Lerp(startValue, panel1.rect.width * targetValue01, timeElapsed / duration);
            Vector2 size = target.sizeDelta;
            size.x = newValue;
            target.sizeDelta = size;

            timeElapsed += Time.deltaTime;
            yield return null;
        }

        // Ujisti se, že na konci je pøesná cílová hodnota
        Vector2 finalSize = target.sizeDelta;
        finalSize.x = panel1.rect.width * targetValue01;
        target.sizeDelta = finalSize;
    }

    public void SetText(ItemType itemType, int current, int max)
    {
        TextMeshProUGUI target = null;
        switch (itemType)
        {
            case ItemType.Wood:
                target = text1;
                break;
            case ItemType.Stone:
                target = text2;
                break;
            case ItemType.Ore:
                target = text3;
                break;
            default:
                Debug.LogWarning($"SetText: Neznámý ItemType {itemType}");
                return;
        }
        if (target == null)
        {
            Debug.LogWarning($"SetText: Nenalezeno textové pole pro {itemType}");
            return;
        }
        target.text = $"{current}/{max}";
    }

    public void HideThirdSource()
    {
        if (HideThirdSourcePanel == null) {
            Debug.LogError("ThirdPanel not set");
            return; 
        }

        Vector2 size = PanelFor3Source.sizeDelta;
        size.y =  PanelFor2Source.sizeDelta.y;
        PanelFor3Source.sizeDelta = size;
    }
}
