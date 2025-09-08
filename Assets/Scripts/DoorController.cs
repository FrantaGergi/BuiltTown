using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private float distanceToOpen = 3f;         // Vzdálenost, kdy se dveøe zaènou otevírat
    [SerializeField] private float openSpeed = 2f;             // Rychlost otevírání
    [SerializeField] private float distanceMultiplier = 2f;    // Jak daleko se dveøe otevøou
    [SerializeField] private bool leftRight = true;            // true = otevøení doleva, false = doprava
    [SerializeField] private Transform player;                 // Reference na hráèe

    public bool isOpen { get; private set; }                  // Stav dveøí

    private Vector3 closedPosition; // Pevná zavøená pozice dveøí
    private Vector3 openPosition;   // Cílová pozice dveøí pøi otevøení

    private void Start()
    {
        // Uložíme startovní pozici dveøí
        closedPosition = transform.position;

        // Smìr otevøení podle LeftRight
        Vector3 offset = leftRight ? transform.right : -transform.right;

        // Vypoèítáme pozici, kam se dveøe otevøou
        openPosition = closedPosition + offset * distanceMultiplier;
    }

    private void Update()
    {
        // Vzdálenost k hráèi poèítáme vždy od zavøené pozice dveøí
        float distance = Vector3.Distance(closedPosition, player.position);

        if (distance < distanceToOpen)
            OpenDoor();
        else
            CloseDoor();
    }

    private void OpenDoor()
    {
        // Plynulé otevøení dveøí
        transform.position = Vector3.MoveTowards(transform.position, openPosition, openSpeed * Time.deltaTime);

        // Pokud jsou dveøe dost blízko otevøené pozice, oznaèíme je jako otevøené
        if (Vector3.Distance(transform.position, openPosition) < 0.01f)
            isOpen = true;
    }

    private void CloseDoor()
    {
        // Plynulé zavøení dveøí
        transform.position = Vector3.MoveTowards(transform.position, closedPosition, openSpeed * Time.deltaTime);

        // Pokud jsou dveøe dost blízko zavøené pozice, oznaèíme je jako zavøené
        if (Vector3.Distance(transform.position, closedPosition) < 0.01f)
            isOpen = false;
    }

    private void OnDrawGizmosSelected()
    {
        // Vizualizace vzdálenosti pro otevøení ve scénì
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanceToOpen);
    }
}
