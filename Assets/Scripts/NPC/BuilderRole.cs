using UnityEngine;

public class BuilderRole : NPCRoleBase
{
    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;

    // Externì pøiøazený building site (èeká na povel)
    private IBuildingSite assignedBuilding;

    // Interní cíl (mùže být stejný jako assignedBuilding)
    private IBuildingSite targetBuilding;

    private enum State { Idle, MovingToBuilding, Building }
    private State state = State.Idle;
    private State previousState = State.Idle;

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                if (assignedBuilding != null)
                {
                    // pokud máme pøiøazený úkol, zaèneme ho vykonávat
                    StartAssignedBuildingWork();
                }
                else if (autoSearch)
                {
                    FindBuilding();
                }
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

        // Aktualizuj UI status jen pøi zmìnì stavu
        if (state != previousState)
        {
            UpdateUiStatus(state);
            previousState = state;
        }
    }

    private void UpdateUiStatus(State s)
    {
        switch (s)
        {
            case State.Idle:
                if (assignedBuilding != null)
                    npc?.SetUiStatus("Pøipraven k práci");
                else
                    npc?.SetUiStatus("Neèinný");
                break;
            case State.MovingToBuilding:
                npc?.SetUiStatus("Jdu na stavbu");
                break;
            case State.Building:
                npc?.SetUiStatus("Staví");
                break;
        }
    }

    // Externí API: pøiøaï building site a okamžitì ho zaèni obsluhovat
    public void AssignBuildingSite(IBuildingSite site)
    {
        if (site == null) return;
        assignedBuilding = site;
        targetBuilding = site;
        npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
        state = State.MovingToBuilding;
    }

    // Externí API: zruší pøiøazení a vrátí roli do idle stavu
    public void ClearAssignment()
    {
        assignedBuilding = null;
        targetBuilding = null;
        state = State.Idle;
        npc.Stop();
    }

    private void StartAssignedBuildingWork()
    {
        if (assignedBuilding == null) return;
        targetBuilding = assignedBuilding;
        npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
        state = State.MovingToBuilding;
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
