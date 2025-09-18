using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class InteractManager : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;
    [SerializeField] private TextMeshProUGUI DescriptionText;
    [SerializeField] private InventoryManager inventoryManager;



    void Update() => ShowDescription();

    private void ShowDescription()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, rayDistance, interactableLayer.value))
        {
            IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                string text = interactable.GetInteractionDescription();
                DescriptionText.text = text != null ? text + " (E)" : "Nemá popis";
            }
        }
        else
        {
            DescriptionText.text = "";
        }
    }
    // This method is called from the Input System when the interact action is performed
    public void TryInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return; //reaguj jen na performed

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = camera.ScreenPointToRay(mousePos);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, rayDistance, interactableLayer.value))
        {
            IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();
            if (interactable != null)
            {
                ICommand command = interactable.GetInteractionCommand(inventoryManager);
                command?.Execute();
            }
        }
    }
}
