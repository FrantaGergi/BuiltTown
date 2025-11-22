using UnityEngine;
using System.Collections;

public class UIBuildingStorage : MonoBehaviour, IInteractable
{
    public Canvas threeDCanvas;
    public Transform player;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform centerPoint;
    [SerializeField, Range(0,20)] private int radius = 5;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private InteractManager interactManager; // nastav v inspektoru nebo najdi v Start()
    [SerializeField] private BuildingStorage buildingStorage; // reference na budovu
    private Coroutine scaleCoroutine;
    private bool isVisible = false;

    // uložíme pùvodní layer hráèe, abychom ho mohli vrátit
    private int originalPlayerLayer = -1;
    private bool playerLayerSaved = false;

    void Start()
    {
        if (threeDCanvas != null)
        {
            threeDCanvas.enabled = false;
            threeDCanvas.transform.localScale = Vector3.zero;
        }
        if (interactManager == null)
            interactManager = FindFirstObjectByType<InteractManager>();
        if(player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

    }

    private void OnDrawGizmos()
    {
        if (centerPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(centerPoint.position, radius);
        }
    }

    void Update()
    {
        if (player == null || threeDCanvas == null)
            return;

        bool shouldShow = Vector3.Distance(centerPoint.position, player.position) < radius;

        if (shouldShow && !isVisible)
        {
            isVisible = true;
            threeDCanvas.enabled = true;
            // registrovat se jako forced target, aby E šel na tento UIBuildingStorage i bez aimu
            interactManager?.SetForcedTarget(this);

            // zmìnit layer hráèe na "UI" (uložíme pùvodní)
            SaveAndSetPlayerLayerToUI();

            StartScaleAnimation(new Vector3(0.00499999989f, 0.00499999989f, 0.00499999942f));
        }
        else if (!shouldShow && isVisible)
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
            // odregistrovat forced target, pokud jsem to já
            interactManager?.ClearForcedTarget(this);

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

        originalPlayerLayer = LayerMask.NameToLayer("Default");
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

    // IInteractable impl
    public void Interact(InteractManager interactor, InteractManager.InteractAction action)
    {
        if (action == InteractManager.InteractAction.EStart)
        {
            // sem vlož otevøení menu stavby / potvrzení
            buildingStorage?.OnInteract(interactor, true);
            // napø. OpenBuildingMenu();
        }
        if (action == InteractManager.InteractAction.EEnd)
        {
            // sem vlož zavøení menu stavby / zrušení
            buildingStorage?.OnInteract(interactor, false);
            // napø. CloseBuildingMenu();
        }
        if (action == InteractManager.InteractAction.HoldStart) // try hit log/ore/stone..
        {
            buildingStorage?.TryHitInGame();
        }
        if (action == InteractManager.InteractAction.HoldEnd)
        {
            // pøípadnì ukonèit nìco
        }
    }

    public void OnHoverEnter(InteractManager interactor)
    {
        // volitelnì vizuální zvýraznìní
        buildingStorage?.OnHoverEnter(interactor);

    }

    public void OnHoverExit()
    {
        // zrušit zvýraznìní
    }
}
