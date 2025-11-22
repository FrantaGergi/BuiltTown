using UnityEngine;

public class BuilderRole : NPCRoleBase
{
    public float searchRadius = 20f;

    private IBuildingSite targetBuilding;

    private enum State { Idle, MovingToBuilding, Building }
    private State state = State.Idle;

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                FindBuilding();
                break;
            case State.MovingToBuilding:
                if (targetBuilding == null) { state = State.Idle; break; }
                if (npc.IsAtDestination()) { state = State.Building; npc.Stop(); }
                break;
            case State.Building:
                if (targetBuilding == null) { state = State.Idle; break; }
                // builder expects materials to be available near building (handled by collectors)
                // For now, simply wait or animate building
                break;
        }
    }

    private void FindBuilding()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius);
        IBuildingSite best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var bs = c.GetComponent<IBuildingSite>();
            if (bs == null) continue;
            // if building needs anything
            if (bs.NeedsResource(ItemType.Wood) || bs.NeedsResource(ItemType.Stone) || bs.NeedsResource(ItemType.Ore))
            {
                float d = Vector3.Distance(transform.position, ((MonoBehaviour)bs).transform.position);
                if (d < bestDist) { bestDist = d; best = bs; }
            }
        }

        if (best != null)
        {
            targetBuilding = best;
            npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
            state = State.MovingToBuilding;
        }
    }
}
