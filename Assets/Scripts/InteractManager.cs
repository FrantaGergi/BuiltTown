using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InteractManager : MonoBehaviour
{
    public float interactDistance = 3f;
    private Camera cam;
    private IInteractable currentTarget;

    public LayerMask interactMask;

    private bool isHolding = false; // aby hráè nemohl cheatovat

    [Header("Tool Animator"), SerializeField]
    private Animator toolAnimator; // pro check animace pøi držení
    [Header("Player Equipment"), SerializeField]
    private PlayerEquipment playerEquipment;
    [Header("Map manager"), SerializeField]
    private ResourceMapManager resourceMapManager;
    [Header("Inventory manager"), SerializeField]
    private InventoryManager inventoryManager;


    public enum InteractAction
    {
        None,
        E,
        R,
        HoldStart,
        Hold,
        HoldEnd
    }

    private void Awake()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        // Každý frame kontroluje, na co hráè míøí
        TryInteract(InteractAction.None);
    }

    public void OnHoldInteract(InputAction.CallbackContext ctx)
    {
        if (ctx.started) // zaèátek držení
        {
            TryInteract(InteractAction.HoldStart);
        }
        else if (ctx.performed) // bìhem držení (triggery u "Hold")
        {
            TryInteract(InteractAction.Hold);
        }
        else if (ctx.canceled) // puštìní tlaèítka
        {
            TryInteract(InteractAction.HoldEnd);
        }
    }


    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryInteract(InteractAction.E);
    }

    public void OnSecondaryInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;
        TryInteract(InteractAction.R);
    }

    private void TryInteract(InteractAction action)
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactMask))
        {
            var interactable = hit.collider.GetComponent<IInteractable>();

            if (interactable != currentTarget)
            {
                // když odcházím z targetu a byl aktivní hold  stopni ho
                if (currentTarget != null && isHolding)
                {
                    currentTarget.Interact(this, InteractAction.HoldEnd);
                    isHolding = false;
                }

                currentTarget?.OnHoverExit();
                currentTarget = interactable;
                currentTarget?.OnHoverEnter();
            }

            if (action != InteractAction.None && interactable != null)
            {
                // trackujeme, jestli držíme
                if (action == InteractAction.HoldStart) isHolding = true;
                if (action == InteractAction.HoldEnd) isHolding = false;

                interactable.Interact(this, action);
            }
        }
        else
        {
            if (currentTarget != null)
            {
                // pokud ztratíme cíl uprostøed držení
                if (isHolding)
                {
                    currentTarget.Interact(this, InteractAction.HoldEnd);
                    isHolding = false;
                }

                currentTarget.OnHoverExit();
                currentTarget = null;
            }
        }
    }

    public void ClearCurrentTarget(IInteractable target = null)
    {
        // Pokud je pøedán target, smaže jen pokud sedí s currentTarget
        if (target == null || currentTarget == target)
        {
            if (currentTarget != null)
            {
                if (isHolding)
                {
                    currentTarget.Interact(this, InteractAction.HoldEnd);
                    isHolding = false;
                }
                currentTarget.OnHoverExit();
                currentTarget = null;
            }
        }
    }

    public Animator GetToolAnimator()
    {
        return toolAnimator;
    }
    public PlayerEquipment GetPlayerEquipment()
    {
        return playerEquipment;
    }
    public ResourceMapManager GetResourceMapManager()
    {
        return resourceMapManager;
    }
    public InventoryManager GetInventoryManager()
    {
        return inventoryManager;
    }
}