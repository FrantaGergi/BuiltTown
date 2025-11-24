using UnityEngine;
using System.Collections;

public class MinerRole : NPCRoleBase
{
    public float searchRadius = 20f;
    public float mineInterval = 2f;
    public int mineAmountPerInterval = 1;

    private IResourceSource targetSource;
    private float mineTimer = 0f;

    private enum State { Idle, MovingToSource, Mining }
    private State state = State.Idle;

    void Update()
    {
        switch (state)
        {
            case State.Idle:
                FindResourceNode();
                break;
            case State.MovingToSource:
                if (targetSource == null) { state = State.Idle; break; }
                if (npc.IsAtDestination())
                {
                    state = State.Mining;
                    mineTimer = 0f;
                    npc.Stop();
                }
                break;
            case State.Mining:
                if (targetSource == null || !targetSource.CanMine()) { state = State.Idle; break; }
                mineTimer += Time.deltaTime;
                if (mineTimer >= mineInterval)
                {
                    targetSource.MineOnce(mineAmountPerInterval);
                    mineTimer = 0f;
                }
                break;
        }
    }

    private void FindResourceNode()
    {
        // naive search using Physics.OverlapSphere for ResourceNode components
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius);
        IResourceSource best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var rs = c.GetComponent<IResourceSource>();
            if (rs == null) continue;
            if (!rs.CanMine()) continue;
            float d = Vector3.Distance(transform.position, ((MonoBehaviour)rs).transform.position);
            if (d < bestDist) { bestDist = d; best = rs; }
        }

        if (best != null)
        {
            targetSource = best;
            npc.MoveTo(((MonoBehaviour)targetSource).transform.position);
            state = State.MovingToSource;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}
