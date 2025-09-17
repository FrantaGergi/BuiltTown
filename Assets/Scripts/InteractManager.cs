using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractManager : MonoBehaviour
{
    [SerializeField] private Camera camera;
    [SerializeField] private float rayDistance = 5f;
    [SerializeField] private LayerMask interactableLayer;

    [SerializeField] private TextMeshProUGUI DescriptionText;
    [SerializeField] private InventoryManager inventoryManager;
    void Start()
    {
        DescriptionText.text = "";
    }
    void Update()
    {
        Interact();
    }

    private void Interact()
    {
       /* if (Input.GetKeyDown(KeyCode.E) && inventoryManager.storageParent.IsOpen)
        {
            inventoryManager.CloseStorage(inventoryManager.storageParent.lastStorage);
            return; // we dont wanna continue if we just wanted close storage -also i thought the inventory-
        }*/

        if (/*!inventoryManager.InventoryParent.IsOpen*/ true)
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
                    if (text != null)
                        DescriptionText.text = text + " (E)";
                    if (text == null)
                        DescriptionText.text = "nemá to text ale je interactable";



                  /*  if (Input.GetKeyDown(KeyCode.E))  // Collect or Open Storage
                    {


                        ICommand command = interactable.GetInteractionCommand(inventoryManager); // only one bro
                        if (command != null)
                            command.Execute();
                    }
                  */
                }
            }
            else
            {
                DescriptionText.text = "";
            }
        }
        else
        {
            DescriptionText.text = "";

        }


    }
}
