using UnityEngine;

public class GroundItem : MonoBehaviour, IGroundItem
{
    [SerializeField] private ItemType type = ItemType.Wood;
    [SerializeField] private SpawnIconBehaviour spawnIconBehaviour;
    public int quantity = 1;
    public int Quantity => quantity;
    public ItemType Type => type;

    private void Start()
    {
        if(spawnIconBehaviour == null)
            spawnIconBehaviour = GetComponent<SpawnIconBehaviour>();

        if(spawnIconBehaviour == null)
            Debug.LogWarning("SpawnIconBehaviour component not found on GroundItem.");

    }

    public void PickUp(Transform npc_pos)
    {
        
        spawnIconBehaviour?.PickUpByNPC(npc_pos);
      
    }
    public void SetQuantity(int qty)
    {
        quantity = qty;
    }
}
