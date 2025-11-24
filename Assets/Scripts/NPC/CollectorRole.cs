using UnityEngine;
using System.Collections.Generic;

public class CollectorRole : NPCRoleBase
{
    public float searchRadius = 20f;
    public int capacity = 5;

    private List<(ItemType type, int amount)> inventory = new();
    private IGroundItem targetItem;
    private IBuildingSite targetBuilding;

    private enum State { Idle, MovingToItem, PickingUp, MovingToBuilding, Depositing }
    private State state = State.Idle;

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                if (inventoryTotal() < capacity)
                    FindGroundItem();
                else
                    FindBuildingAndDeliver();
                break;
            case State.MovingToItem:
                if (targetItem == null) { state = State.Idle; break; }
                if (npc.IsAtDestination()) { state = State.PickingUp; npc.Stop(); }
                break;
            case State.PickingUp:
                if (targetItem == null) { state = State.Idle; break; }
                // pick up
                OnPickUp((GroundItem)((MonoBehaviour)targetItem));
                state = State.Idle;
                break;
            case State.MovingToBuilding:
                if (targetBuilding == null) { state = State.Idle; break; }
                if (npc.IsAtDestination()) { state = State.Depositing; npc.Stop(); }
                break;
            case State.Depositing:
                if (targetBuilding == null) { state = State.Idle; break; }
                DepositAll();
                state = State.Idle;
                break;
        }
    }

    private void FindGroundItem()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius);
        IGroundItem best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var gi = c.GetComponent<IGroundItem>();
            if (gi == null) continue;

            // filter by allowed item types - assume inspector set allowed types via BaseNPC or Role in future
            float d = Vector3.Distance(transform.position, ((MonoBehaviour)gi).transform.position);
            if (d < bestDist) { bestDist = d; best = gi; }
        }

        if (best != null)
        {
            targetItem = best;
            npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
            state = State.MovingToItem;
        }
    }

    public void OnPickUp(GroundItem gi)
    {
        if (gi == null) return;
        if (inventoryTotal() >= capacity) return;

        gi.PickUp(transform);
        inventory.Add((gi.Type, gi.Quantity));
    }

    private void FindBuildingAndDeliver()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius);
        IBuildingSite best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var bs = c.GetComponent<IBuildingSite>();
            if (bs == null) continue;
            // check if building needs any of our carried resources
            foreach (var it in inventory)
            {
                if (bs.NeedsResource(it.type))
                {
                    float d = Vector3.Distance(transform.position, ((MonoBehaviour)bs).transform.position);
                    if (d < bestDist) { bestDist = d; best = bs; }
                    break;
                }
            }
        }

        if (best != null)
        {
            targetBuilding = best;
            npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
            state = State.MovingToBuilding;
        }
    }

    private void DepositAll()
    {
        if (targetBuilding == null) return;

        foreach (var it in inventory)
        {
            if (targetBuilding.NeedsResource(it.type))
            {
                targetBuilding.AddResource(it.type, it.amount);
            }
        }

        inventory.Clear();
    }

    private int inventoryTotal()
    {
        int s = 0;
        foreach (var it in inventory) s += it.amount;
        return s;
    }
}
