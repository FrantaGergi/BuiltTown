using UnityEngine;

public interface IGroundItem
{
    ItemType Type { get; }
    int Quantity { get; }
    void PickUp(Transform npc_pos);
}
