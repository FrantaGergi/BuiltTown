using System.Collections;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;

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
    [SerializeField] private Button buyButton;

    [SerializeField] private UnityEngine.Canvas threeDCanvas;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float rotationSpeed = 10f;

    private Coroutine scaleCoroutine;
    // uložíme pùvodní layer hráèe, abychom ho mohli vrátit
    private int originalPlayerLayer = -1;
    private bool playerLayerSaved = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (threeDCanvas != null)
        {
            threeDCanvas.enabled = false;
            threeDCanvas.transform.localScale = Vector3.zero;
        }
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

    }

    public void SetShowCanvas(bool show, ItemSO itemSO, Sprite icon)
    {
        ShowCanvas = show;
        if (show)
            SetInformation(itemSO, icon);
        
    }


    private void SetInformation(ItemSO itemSO, Sprite icon)
    {
        titleTextMeshPro.text = itemSO.ItemName;
        descriptionTextMeshPro.text = itemSO.description + "costs $" + itemSO.price;
        this.iconImage.sprite = icon;
        multiplierTextMeshPro.text = "x " + (itemSO.gatherAmount).ToString().ToUpper();

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

            StartScaleAnimation(new Vector3(0.000277585f, 0.000277585f, 0.000277585f));
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

        originalPlayerLayer = player.gameObject.layer;
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

