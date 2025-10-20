using UnityEngine;

public class SingleDoorController : MonoBehaviour
{
    [Header("Nastavení")]
    [SerializeField] private Transform door; // odkaz na dveøe
    [SerializeField] private Transform player; // odkaz na hráèe
    [SerializeField] private float openRotationY = -125f; // cílová rotace na ose Y
    [SerializeField] private float closeRotationY = 0f; // výchozí rotace
    [SerializeField] private float openDistance = 3f; // vzdálenost pro otevøení
    [SerializeField] private float smoothSpeed = 3f; // rychlost otvírání/zavírání

    private bool isOpen = false;
    private float currentY;

    void Start()
    {
        if (door == null)
            door = transform; // pokud skript je pøímo na dveøích

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

        currentY = door.localEulerAngles.y;
    }

    void Update()
    {
        float distance = Vector3.Distance(player.position, door.position);
        bool shouldOpen = distance <= openDistance;

        // pokud se stav zmìnil, pøepne isOpen
        if (shouldOpen != isOpen)
            isOpen = shouldOpen;

        // cílový úhel
        float targetY = isOpen ? openRotationY : closeRotationY;

        // plynulé pøecházení mezi úhly
        currentY = Mathf.LerpAngle(currentY, targetY, Time.deltaTime * smoothSpeed);
        Vector3 currentRotation = door.localEulerAngles;
        currentRotation.y = currentY;
        door.localEulerAngles = currentRotation;
    }
}
