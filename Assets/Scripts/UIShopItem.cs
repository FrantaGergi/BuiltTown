using System.Collections;
using TMPro;
using Unity.AppUI.UI;
using Unity.InferenceEngine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Random = UnityEngine.Random;

public class UIShopItem : MonoBehaviour
{
    public bool ShowCanvas { get; private set; } = false;
    private bool isVisible = false;
    private Transform player;

    [Header("References")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleTextMeshPro;
    [SerializeField] private TextMeshProUGUI descriptionTextMeshPro;
    [SerializeField] private TextMeshProUGUI multiplierTextMeshPro;
    [SerializeField] private Image buyButton;

    [SerializeField] private UnityEngine.Canvas threeDCanvas;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Shake on Not Enough Money")]
    [SerializeField] private float shakeDuration = 0.5f;
    [SerializeField] private float shakeMagnitude = 0.001f;

    private Coroutine scaleCoroutine;
    private Coroutine shakeCoroutine;
    // uložíme pùvodní layer hráèe, abychom ho mohli vrátit
    private int originalPlayerLayer = -1;
    private bool playerLayerSaved = false;
    private Vector3 originalCanvasLocalPos;

    private string lastdescriptionText = "";
    private ItemType currentItemType = ItemType.None;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (threeDCanvas != null)
        {
            threeDCanvas.enabled = false;
            threeDCanvas.transform.localScale = Vector3.zero;
            originalCanvasLocalPos = threeDCanvas.transform.localPosition;
        }
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

    }

    public void SetShowCanvas(bool show, ItemSO itemSO, Sprite icon)
    {
        ShowCanvas = show;
        if (show)
            SetInformation(itemSO, icon);

        currentItemType = itemSO.itemType;

    }
    public void SetnotEnoughMoney()
    {
        lastdescriptionText = descriptionTextMeshPro.text;
        descriptionTextMeshPro.text = "Not enough money!";
        if (threeDCanvas != null)
            StartShake();
    }

   
    private void SetInformation(ItemSO itemSO, Sprite icon)
    {
        titleTextMeshPro.text = itemSO.ItemName;
        descriptionTextMeshPro.text = itemSO.description + "costs $" + itemSO.price;
        if(itemSO.gatherAmount == 0)
        {
            multiplierTextMeshPro.text = "";
            this.iconImage.gameObject.SetActive(false);
        }
        else
        {
            this.iconImage.gameObject.SetActive(true);
            if(icon != null)
                this.iconImage.sprite = icon;
            multiplierTextMeshPro.text = "x " + (itemSO.gatherAmount).ToString().ToUpper();
        }

          

    }

    // Update is called once per frame
    void Update()
    {
      

        if (ShowCanvas && !isVisible)
        {
            isVisible = true;
            threeDCanvas.enabled = true;
            // registrovat se jako forced target, aby E šel na tento UIBuildingStorage i bez aimu

            // zmìnit layer hráèe na "UI" (uložíme pùvodní)
            SaveAndSetPlayerLayerToUI();
            if(currentItemType == ItemType.Chopp) // Axe
                StartScaleAnimation(new Vector3(0.000277585f, 0.000277585f, 0.000277585f));
            else if( currentItemType == ItemType.Mine) // Pickaxe
                StartScaleAnimation(new Vector3(0.000116279f, 0.000116279f, 0.000116279f));
            else if(currentItemType == ItemType.None) // NPC 
                StartScaleAnimation(new Vector3(0.003999999f, 0.004f, 0.003999999f));

        }
        else if (!ShowCanvas && isVisible)
        {
            isVisible = false;
            StartScaleAnimation(Vector3.zero, disableOnEnd: true);
            // odregistrovat se (po skonèení animace ještì deaktivujeme -> ClearForcedTarget v konci korutiny)
            // obnovení layeru je provedeno v korutinì po dokonèení animace (viz ScaleCanvas)
        }

        if (isVisible)
        {
            Vector3 targetPosition = new Vector3(player.position.x, threeDCanvas.transform.position.y, player.position.z);
            Vector3 direction = targetPosition - threeDCanvas.transform.position;
            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                threeDCanvas.transform.rotation = Quaternion.Slerp(threeDCanvas.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }

    private void StartScaleAnimation(Vector3 targetScale, bool disableOnEnd = false)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);
        scaleCoroutine = StartCoroutine(ScaleCanvas(targetScale, disableOnEnd));
    }

    private IEnumerator ScaleCanvas(Vector3 targetScale, bool disableOnEnd)
    {
        Vector3 startScale = threeDCanvas.transform.localScale;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            threeDCanvas.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsed / scaleDuration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        threeDCanvas.transform.localScale = targetScale;

        if (disableOnEnd && targetScale == Vector3.zero)
        {
            threeDCanvas.enabled = false;

            // obnovíme pùvodní layer hráèe
            RestorePlayerLayer();
        }
    }

    private void StartShake()
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(ShakeCanvas());
    }

    private IEnumerator ShakeCanvas()
    {
        if (threeDCanvas == null)
            yield break;

        Vector3 startPos = originalCanvasLocalPos;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float progress = elapsed / shakeDuration;
            float damper = 1f - progress; // postupnì menší amplituda
            Vector3 offset = Random.insideUnitSphere * shakeMagnitude * damper;
            // chceme pouze posun v lokálních X/Y pro 2D efekt
            offset.z = 0f;
            threeDCanvas.transform.localPosition = startPos + offset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        threeDCanvas.transform.localPosition = startPos;
        shakeCoroutine = null;
        descriptionTextMeshPro.text = lastdescriptionText;
    }

    private void SaveAndSetPlayerLayerToUI()
    {
        if (player == null) return;
        if (playerLayerSaved) return; // už nastaveno

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer == -1)
        {
            Debug.LogWarning($"{name}: Layer 'UI' nenalezena. Nastavení layeru pøeskoèeno.");
            return;
        }

        originalPlayerLayer = LayerMask.NameToLayer("Player");
        playerLayerSaved = true;
        SetLayerRecursively(player, uiLayer);
    }

    private void RestorePlayerLayer()
    {
        if (player == null) return;
        if (!playerLayerSaved) return;

        SetLayerRecursively(player, originalPlayerLayer);
        playerLayerSaved = false;
        originalPlayerLayer = -1;
    }

    private void SetLayerRecursively(Transform t, int layer)
    {
        if (t == null) return;
        t.gameObject.layer = layer;
        for (int i = 0; i < t.childCount; i++)
            SetLayerRecursively(t.GetChild(i), layer);
    }
  
    public void OnPressE()
    {
        buyButton.color = new Color(100,100,100);
        Invoke(nameof(ResebuttonCollor) , 0.48f);
    }

    private void ResebuttonCollor()
    {
        buyButton.color = Color.white;
    }

    private void OnDisable()
    {
        // zajistíme, že se layer hráèe obnoví pokud by se component deaktivovalo
        RestorePlayerLayer();
    }

    private void OnDestroy()
    {
        RestorePlayerLayer();
    }

}

