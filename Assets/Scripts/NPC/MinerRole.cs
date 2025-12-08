using UnityEngine;
using System.Collections;

public class MinerRole : NPCRoleBase
{
    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;
    public float mineInterval = 2f;
    public int mineAmountPerInterval = 1;

    private IResourceSource targetSource;
    private float mineTimer = 0f;

    // Externí pøiøazení úkolu: souøadnice odkud hledat + typ zdroje
    private Vector3 assignedSourcePos;
    private ItemType assignedResourceType = default;
    private bool hasAssignedSourcePos = false;

    private enum State { Idle, MovingToSource, Mining }
    private State state = State.Idle;

    void Update()
    {
        // Pokud jsme pøiøazeni, ale zdroj už nelze tìžit (nebo budeme mít jiný dùvod), vyèistit assignment
        if (hasAssignedSourcePos && assignedResourceType != default && assignedSourcePos != Vector3.zero)
        {
            // pokud cílový building / podmínky nejsou relevantní pro minera, kontrola se provádí v prùbìhu práce
        }

        switch (state)
        {
            case State.Idle:
                if (hasAssignedSourcePos)
                {
                    StartAssignedMining();
                }
                else if (autoSearch)
                {
                    FindResourceNode();
                }
                break;
            case State.MovingToSource:
                if (targetSource == null) { state = State.Idle; break; }
                if (npc.IsAtDestination())
                {
                    // pokud zdroj už nelze tìžit, najdi jiný
                    if (!targetSource.CanMine())
                    {
                        targetSource = null;
                        state = State.Idle;
                        break;
                    }

                    state = State.Mining;
                    mineTimer = 0f;
                    npc.Stop();
                }
                break;
            case State.Mining:
                if (targetSource == null || !targetSource.CanMine())
                {
                    // zkus najít další dostupný zdroj stejného typu
                    targetSource = null;
                    state = State.Idle;
                    break;
                }

                mineTimer += Time.deltaTime;
                if (mineTimer >= mineInterval)
                {
                    // vykonej jednorázové tìžení
                    targetSource.MineOnce(mineAmountPerInterval);
                    mineTimer = 0f;

                    // pokud byl zdroj vyèerpán po MineOnce, pøepni stav a vyhledej další
                    if (!targetSource.CanMine())
                    {
                        targetSource = null;
                        state = State.Idle;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Pøiøadí minerovi úkol: hledat zdroje typu <paramref name="resourceType"/> kolem <paramref name="sourcePos"/>.
    /// </summary>
    public void AssignMiningTask(Vector3 sourcePos, ItemType resourceType)
    {
        assignedSourcePos = sourcePos;
        assignedResourceType = resourceType;
        hasAssignedSourcePos = true;

        // restartuj pøípadné probíhající akce tak, aby miner okamžitì zaèal plnit nový úkol
        targetSource = null;
        state = State.Idle;
    }

    /// <summary>
    /// Zruší pøiøazený úkol a vrátí roli do idle stavu.
    /// </summary>
    public void ClearAssignment()
    {
        hasAssignedSourcePos = false;
        assignedSourcePos = Vector3.zero;
        assignedResourceType = default;
        targetSource = null;
        state = State.Idle;
        npc.Stop();
    }

    private void StartAssignedMining()
    {
        if (!hasAssignedSourcePos) return;

        // Najdi nejbližší zdroj požadovaného typu kolem assignedSourcePos
        IResourceSource nearest = FindNearestResourceSourceOfType(assignedSourcePos, assignedResourceType);
        if (nearest != null && nearest.CanMine())
        {
            targetSource = nearest;
            npc.MoveTo(((MonoBehaviour)targetSource).transform.position);
            state = State.MovingToSource;
            return;
        }

        // Fallback: pokud nic není kolem assignedSourcePos, zkus prohledat okolí NPC
        IResourceSource fallback = FindNearestResourceSourceOfType(((MonoBehaviour)npc).transform.position, assignedResourceType);
        if (fallback != null && fallback.CanMine())
        {
            targetSource = fallback;
            npc.MoveTo(((MonoBehaviour)targetSource).transform.position);
            state = State.MovingToSource;
            return;
        }

        // Pokud nic nenalezeno, zrušíme assignment (nebo mùžeme èekat/delší retry logiku)
        Debug.Log($"MinerRole: žádný zdroj typu {assignedResourceType} nalezen pro assignedSourcePos {assignedSourcePos}.");
        // Místo automatického zrušení mùžeš chtít nechat miner èekat a opakovat vyhledávání pozdìji.
        ClearAssignment();
    }

    private IResourceSource FindNearestResourceSourceOfType(Vector3 origin, ItemType type)
    {
        Collider[] cols = Physics.OverlapSphere(origin, searchRadius);
        IResourceSource best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var rs = c.GetComponent<IResourceSource>();
            if (rs == null) continue;
            if (rs.Type != type) continue;
            if (!rs.CanMine()) continue;

            float d = Vector3.Distance(origin, ((MonoBehaviour)rs).transform.position);
            if (d < bestDist) { bestDist = d; best = rs; }
        }
        return best;
    }

    private void FindResourceNode()
    {
        // pùvodní automatické chování, vyhledávání jakéhokoliv dostupného zdroje
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
