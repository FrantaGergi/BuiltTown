using UnityEngine;
using System.Collections;

public class SpawnIconBehaviour : MonoBehaviour
{
    [Header("Hop nastavení")]
    public float radius = 0.5f;        // vzdálenost od kmene pøi startu
    public float hopDistance = 1.5f;   // kam to doletí
    public float hopHeight = 1f;       // výška oblouku
    public float hopTime = 0.6f;       // rychlost letu
    public LayerMask groundMask;

    [Header("Idle fáze")]
    public float idleTime = 1.5f;      // doba než zmizí
    public float rotationSpeed = 150f; // otáèení po dopadu


    [Header("Pickup")]
    public float pickupRange = 2.5f;     // jak blízko musí být hráè
    public float jumpToPlayerTime = 0.4f; // jak rychle pøiletí
    public Transform player;             // reference na hráèe (mùže se doplnit dynamicky)

    private bool pickedUp = false;
    private InventoryManager playerInventory;
    private int amount;
    private ItemSO itemSO;

    // collider used for NPC detection / trigger pickup
    private Collider pickupCollider;

    public void SetAndStart(Transform player, InventoryManager inventoryManager, int ammount, ItemSO itemSO)
    {
        this.player = player;
        this.playerInventory = inventoryManager;
        this.amount = ammount;
        this.itemSO = itemSO;

        // ensure there's a collider to be used as trigger for NPC detection
        pickupCollider = GetComponent<Collider>();
        pickupCollider.isTrigger = false;
        pickupCollider.enabled = false;

        StartCoroutine(HopAndIdle());

    }

    private IEnumerator HopAndIdle()
    {
        // === Hop jako døív ===
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + dir * hopDistance;

        Vector3 groundCheck = endPos + Vector3.up * 5f;
        if (Physics.Raycast(groundCheck, Vector3.down, out RaycastHit hit, 10f, groundMask))
            endPos.y = hit.point.y + 0.15f;
        else
            endPos.y = startPos.y;

        float t = 0f;
        float totalTime = hopTime + Random.Range(-0.1f, 0.15f);
        float height = hopHeight * Random.Range(0.8f, 1.2f);
        Quaternion startRot = transform.rotation;
        Quaternion endRot = Quaternion.Euler(
            0,
            Random.Range(0, 360f),
            0
        );

        // ensure collider is trigger while on ground so NPCs can detect it
        if (pickupCollider != null)
        {
            pickupCollider.isTrigger = true;
            pickupCollider.enabled = true;
        }

        while (t < totalTime)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / totalTime);
            float y = Mathf.Sin(normalized * Mathf.PI) * height;
            Vector3 pos = Vector3.Lerp(startPos, endPos, normalized);
            pos.y += y;
            transform.position = pos;
            transform.rotation = Quaternion.Slerp(startRot, endRot, normalized);
            yield return null;
        }

        // === Idle fáze + èekání na hráèe ===
        Vector3 restPos = endPos;
        float timer = 0f;

        while (timer < idleTime && !pickedUp)
        {
            timer += Time.deltaTime;

            // bounce efekt
            float bounce = Mathf.Sin(timer * 4f) * 0.03f * (1f - timer / idleTime);
            transform.position = restPos + Vector3.up * bounce;
            transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);

            // check hráèe
            if (player && Vector3.Distance(transform.position, player.position) < pickupRange)
            {
                StartCoroutine(JumpToPlayer());
                yield break;
            }

            yield return null;
        }

        if (!pickedUp)
            Destroy(gameObject);
    }

    private IEnumerator JumpToPlayer()
    {
        pickedUp = true;

        // disable collider so NPCs won't detect it while flying to player
        if (pickupCollider != null)
            pickupCollider.enabled = false;

        Vector3 start = transform.position;
        Vector3 startScale = transform.localScale;

        float t = 0f;
        while (t < jumpToPlayerTime)
        {
            t += Time.deltaTime;
            float normalized = Mathf.Clamp01(t / jumpToPlayerTime);

            // aktuální pozice hráèe (pohyblivý cíl)
            Vector3 target = player.position + Vector3.up * 1.2f;
            // target += (Vector3.up + player.forward) * 0.3f; // mírnì pøed hráèe
            target.z += 0.6f; // mírný offset dopøedu

            // obloukový pohyb
            float yOffset = Mathf.Sin(normalized * Mathf.PI) * 0.5f;
            Vector3 pos = Vector3.Lerp(start, target, normalized);
            pos.y += yOffset;
            transform.position = pos;

            // zmenšování
            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, normalized);

            yield return null;
        }

        // tady mùžeš pøidat akci po doruèení (napø. pøidání resource)
        // playerInventory.Add("Wood", 1);
        playerInventory.AddResourceToHotbar(itemSO, amount);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickedUp) return;

        // pokud do triggeru vstoupí NPC s CollectorRole, necháme ho item sebrat
        var collector = other.GetComponentInParent<CollectorRole>();
        if (collector != null)
        {
            // stop any running coroutines so we don't race with JumpToPlayer
            StopAllCoroutines();

            // try to find GroundItem component on this prefab
            var groundItem = GetComponent<GroundItem>();
            if (groundItem != null)
            {
                pickedUp = true;
                groundItem.OnPickedByCollector(collector);
            }
            else
            {
                // fallback: if no GroundItem, try to add directly using InventoryManager on collector if available
                // CollectorRole should implement OnPickUp(GroundItem) - without GroundItem we can't call it, so try to add to building inventory if collector exposes method
                // As fallback, just destroy the object
                pickedUp = true;
                Destroy(gameObject);
            }
        }
    }
}
