using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI komponenta pro zobrazení obsahu ResourceHolderu.
/// Umístìna na Canvas v 3D prostoru.
/// </summary>
public class ResourceHolderUI : MonoBehaviour
{
    [Header("Text References")]
    [SerializeField] private TextMeshProUGUI woodCountText;
    [SerializeField] private TextMeshProUGUI stoneCountText;
    [SerializeField] private TextMeshProUGUI oreCountText;
    [Header("Canvas References")]
    [SerializeField] private Canvas holderCanvas;

    [Header("Icon References (Optional)")]
    [SerializeField] private Image woodIcon;
    [SerializeField] private Image stoneIcon;
    [SerializeField] private Image oreIcon;

    [Header("Settings")]
    [SerializeField] private bool hideWhenEmpty = false;
    [SerializeField] private Color emptyColor = Color.gray;
    [SerializeField] private Color normalColor = Color.white;


    private Transform player;
    // uložíme pùvodní layer hráèe, abychom ho mohli vrátit
    private int originalPlayerLayer = -1;
    private bool playerLayerSaved = false;


    private void Start()
    {
        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    /// <summary>
    /// Aktualizuje zobrazení všech zdrojù.
    /// </summary>
    public void UpdateDisplay(int wood, int stone, int ore)
    {
        UpdateSingleResource(woodCountText, woodIcon, wood);
        UpdateSingleResource(stoneCountText, stoneIcon, stone);
        UpdateSingleResource(oreCountText, oreIcon, ore);

        if (hideWhenEmpty)
        {
            bool isEmpty = (wood == 0 && stone == 0 && ore == 0);
            gameObject.SetActive(!isEmpty);
        }
    }

    private void UpdateSingleResource(TextMeshProUGUI text, Image icon, int count)
    {
        if (text != null)
        {
            text.text = count.ToString();
            text.color = count > 0 ? normalColor : emptyColor;
        }

        if (icon != null)
        {
            icon.color = count > 0 ? normalColor : emptyColor;
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

    private void OnDisable()
    {
        // zajistíme, že se layer hráèe obnoví pokud by se component deaktivovalo
        RestorePlayerLayer();
    }

    private void OnDestroy()
    {
        RestorePlayerLayer();
    }
    /// <summary>
    /// Skryje celé UI.
    /// </summary>
    public void HideUI()
    {
        holderCanvas.gameObject.SetActive(false);
        RestorePlayerLayer();
    }

    /// <summary>
    /// Zobrazí celé UI.
    /// </summary>
    public void ShowUI()
    {
        holderCanvas.gameObject.SetActive(true);
        SaveAndSetPlayerLayerToUI();

    }
}