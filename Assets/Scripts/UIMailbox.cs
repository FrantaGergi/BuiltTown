using System.Collections;
using UnityEngine;

public class UIMailbox : MonoBehaviour, IInteractable
{
    public Transform player;

    [Header("References")]
    public Canvas threeDCanvas;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private Transform centerPoint;
    [SerializeField, Range(0, 20)] private int radius = 5;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] public InteractManager interactManager;
    [SerializeField] private Mailbox mailbox; // reference na mailbox

    private Coroutine scaleCoroutine;
    private bool isVisible = false;

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

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }


    void Update()
    {
        if (player == null || threeDCanvas == null || centerPoint == null)
            return;

        // --- IGNORUJEME Y, aby se to nespouštìlo jen pøi skoku ---
        Vector2 playerPos = new Vector2(player.position.x, player.position.z);
        Vector2 centerPos = new Vector2(centerPoint.position.x, centerPoint.position.z);

        bool shouldShow = Vector2.Distance(playerPos, centerPos) < radius;


        // ------ Zobrazení ------
        if (shouldShow && !isVisible)
        {
            isVisible = true;
            threeDCanvas.enabled = true;

            interactManager?.SetForcedTarget(this);
            SaveAndSetPlayerLayerToUI();

            StartScaleAnimation(new Vector3(0.005f, 0.005f, 0.005f));
        }
        // ------ Skrytí ------
        else if (!shouldShow && isVisible)
        {
            isVisible = false;
            StartScaleAnimation(Vector3.zero, disableOnEnd: true);
        }


        // ------ Otáèení smìrem k hráèi ------
        if (isVisible)
        {
            Vector3 targetPos = new Vector3(player.position.x, threeDCanvas.transform.position.y, player.position.z);
            Vector3 direction = targetPos - threeDCanvas.transform.position;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction);
                threeDCanvas.transform.rotation = Quaternion.Slerp(
                    threeDCanvas.transform.rotation,
                    targetRot,
                    Time.deltaTime * rotationSpeed
                );
            }
        }
    }


    // --- Animace scale ---
    private void StartScaleAnimation(Vector3 targetScale, bool disableOnEnd = false)
    {
        if (scaleCoroutine != null)
            StopCoroutine(scaleCoroutine);

        scaleCoroutine = StartCoroutine(ScaleCanvas(targetScale, disableOnEnd));
    }

    private IEnumerator ScaleCanvas(Vector3 targetScale, bool disableOnEnd)
    {
        if (threeDCanvas == null)
            yield break;

        Vector3 startScale = threeDCanvas.transform.localScale;
        float t = 0f;

        while (t < scaleDuration)
        {
            threeDCanvas.transform.localScale = Vector3.Lerp(startScale, targetScale, t / scaleDuration);
            t += Time.deltaTime;
            yield return null;
        }

        threeDCanvas.transform.localScale = targetScale;

        if (disableOnEnd && targetScale == Vector3.zero)
        {
            threeDCanvas.enabled = false;

            interactManager?.ClearForcedTarget(this);
            RestorePlayerLayer();
        }
    }


    // --- Layer management ---
    private void SaveAndSetPlayerLayerToUI()
    {
        if (playerLayerSaved) return;

        if (player == null)
        {
            Debug.LogWarning($"{name}: Nelze nastavit layer hráèe, reference 'player' == null.");
            return;
        }

        int uiLayer = LayerMask.NameToLayer("UI");
        if (uiLayer == -1)
        {
            Debug.LogWarning($"{name}: Layer 'UI' nenalezen.");
            return;
        }

        originalPlayerLayer = player.gameObject.layer;
        playerLayerSaved = true;

        SetLayerRecursively(player, uiLayer);
    }

    private void RestorePlayerLayer()
    {
        if (!playerLayerSaved) return;

        // pokud hráè už není k dispozici, jen vyèistíme flagy a vrátíme se
        if (player == null)
        {
            playerLayerSaved = false;
            originalPlayerLayer = -1;
            return;
        }

        SetLayerRecursively(player, originalPlayerLayer);
        playerLayerSaved = false;
        originalPlayerLayer = -1;
    }

    private void SetLayerRecursively(Transform t, int layer)
    {
        // bezpeènì kontrolujeme null/destroyed object (Unity overloaduje ==)
        if (t == null) return;

        // dalšímu pøístupu zabráníme, pokud je objekt znièený
        if (t.gameObject == null) return;

        t.gameObject.layer = layer;

        // iterování dìtí bezpeènì
        for (int i = 0; i < t.childCount; i++)
        {
            Transform child;
            try
            {
                child = t.GetChild(i);
            }
            catch
            {
                // ochrana v pøípadì, že se struktura zmìnila bìhem iterace
                continue;
            }

            if (child == null) continue;
            SetLayerRecursively(child, layer);
        }
    }


    // --- Bezpeènost ---
    private void OnDisable() => RestorePlayerLayer();
    private void OnDestroy() => RestorePlayerLayer();


    // ------------ IInteractable IMPLEMENTACE -------------------

    public void Interact(InteractManager interactor, InteractManager.InteractAction action)
    {
        if (action == InteractManager.InteractAction.EStart)
        {
            // otevøít mailbox UI
            Debug.Log("Otevøení mail");
            mailbox.OnOpenMailbox(interactManager.GetUIBuildingMailboxController());

        }

        if (action == InteractManager.InteractAction.EEnd)
        {
        }

        if (action == InteractManager.InteractAction.HoldStart)
        {
        }

        if (action == InteractManager.InteractAction.HoldEnd)
        {
        }
    }

    public void OnHoverEnter(InteractManager interactor)
    {
        // highlight
    }

    public void OnHoverExit()
    {
        // unhighlight
    }
}
