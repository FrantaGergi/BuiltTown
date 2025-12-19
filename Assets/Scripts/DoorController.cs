using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float openSpeed = 2f;
    [SerializeField] private float distanceMultiplier = 2f;
    [SerializeField] private bool leftRight = true;

    [Header("Trigger Settings")]
    [SerializeField] private LayerMask triggerMask;
    // V Inspectoru  Player a NPC vrstvy

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private int entitiesInside = 0;
    public bool isOpen { get; private set; }

    private void Start()
    {
        closedPosition = transform.position;

        Vector3 offset = leftRight ? transform.right : -transform.right;
        openPosition = closedPosition + offset * distanceMultiplier;
    }

    private void Update()
    {
        if (entitiesInside > 0)
            OpenDoor();
        else
            CloseDoor();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (IsValidTrigger(other.gameObject))
            entitiesInside++;
    }

    private void OnTriggerExit(Collider other)
    {
        if (IsValidTrigger(other.gameObject))
            entitiesInside--;
    }

    private bool IsValidTrigger(GameObject obj)
    {
        bool isInLayerMask = (triggerMask.value & (1 << obj.layer)) != 0;

        // Pokud jde o Player a zároveò splòuje vrstvu, vyvolej event (napø. v Inspectoru pøipoj GameServices a metodu)
        if (isInLayerMask && obj.CompareTag("Player"))
        {
            GameServices.I.OnShopEntered();
        }

        return isInLayerMask;
    }

    private void OpenDoor()
    {
        transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, openPosition) < 0.01f)
            isOpen = true;
    }

    private void CloseDoor()
    {
        transform.position = Vector3.MoveTowards(transform.position, closedPosition, openSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, closedPosition) < 0.01f)
            isOpen = false;
    }
}
