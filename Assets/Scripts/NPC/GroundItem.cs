using UnityEngine;

public class GroundItem : MonoBehaviour, IGroundItem
{
    [SerializeField] private ItemType type = ItemType.Wood;
    [SerializeField] private int quantity = 1;

    public ItemType Type => type;
    public int Quantity => quantity;

    public void PickUp()
    {
        // could notify a central manager; for now just destroy
        Destroy(gameObject);
    }

    public void OnPickedByCollector(CollectorRole collector)
    {
        // collector should call this to add to inventory
        collector?.OnPickUp(this);
        Destroy(gameObject);
    }
}
