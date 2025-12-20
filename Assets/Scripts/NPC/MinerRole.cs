using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class MinerRole : NPCRoleBase
{
    [Header("References")]
    private NPCAnimationController animController;
    private AudioSource audioSource;

    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;
    public float mineInterval = 2f;
    public int mineAmountPerInterval = 1;

    [Header("Animation")]
    [SerializeField] private float miningAnimationSpeed = 1f;
    [SerializeField] private bool playSwingSound = true;

    private IResourceSource targetSource;
    private float mineTimer = 0f;

    // External assignment
    private Vector3 assignedSourcePos;
    private ItemType assignedResourceType = default;
    private bool hasAssignedSourcePos = false;

    private enum State { Idle, MovingToSource, Mining }
    private State state = State.Idle;
    private State previousState = State.Idle;

    protected override void Awake()
    {
        base.Awake();
        animController = GetComponent<NPCAnimationController>();
        audioSource = GetComponent<AudioSource>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<NPCAnimationController>();
            Debug.LogWarning($"MinerRole: Added NPCAnimationController to {gameObject.name}");
        }
    }

    private void Update()
    {
        switch (state)
        {
            case State.Idle:
                if (hasAssignedSourcePos)
                    StartAssignedMining();
                else if (autoSearch)
                    FindResourceNode();
                break;

            case State.MovingToSource:
                if (targetSource == null)
                {
                    TransitionToIdle();
                    break;
                }

                if (npc.IsAtDestination())
                {
                    if (!targetSource.CanMine())
                    {
                        targetSource = null;
                        TransitionToIdle();
                        break;
                    }

                    TransitionToMining();
                }
                break;

            case State.Mining:
                if (targetSource == null || !targetSource.CanMine())
                {
                    targetSource = null;
                    TransitionToIdle();
                    break;
                }

                // Look at the resource while mining
                npc.LookAtTarget(((MonoBehaviour)targetSource).transform.position);

                mineTimer += Time.deltaTime;
                if (mineTimer >= mineInterval)
                {
                    PerformMining();
                    mineTimer = 0f;

                    if (!targetSource.CanMine())
                    {
                        targetSource = null;
                        TransitionToIdle();
                    }
                }
                break;
        }

        // Update UI on state change
        if (state != previousState)
        {
            UpdateUiStatus(state);
            previousState = state;
        }
    }

    #region State Transitions

    private void TransitionToIdle()
    {
        state = State.Idle;
        npc.Stop();
        animController?.StopMining();
    }

    private void TransitionToMining()
    {
        state = State.Mining;
        mineTimer = 0f;
        npc.Stop();
        animController?.StartMining(miningAnimationSpeed);
    }

    #endregion

    #region Mining Logic

    private void PerformMining()
    {
        targetSource.MineOnce(mineAmountPerInterval, audioSource);

        // Optional: Play additional swing sound
        if (playSwingSound && audioSource != null)
        {
            // Sound is already played in MineOnce, but you can add extra effects here
        }
    }

    #endregion

    #region Assignment API

    public void AssignMiningTask(Vector3 sourcePos, ItemType resourceType)
    {
        assignedSourcePos = sourcePos;
        assignedResourceType = resourceType;
        hasAssignedSourcePos = true;

        targetSource = null;
        TransitionToIdle();
    }

    public void ClearAssignment()
    {
        hasAssignedSourcePos = false;
        assignedSourcePos = Vector3.zero;
        assignedResourceType = default;
        targetSource = null;
        TransitionToIdle();
    }

    #endregion

    #region Search Logic

    private void StartAssignedMining()
    {
        if (!hasAssignedSourcePos) return;

        IResourceSource nearest = FindNearestResourceSourceOfType(assignedSourcePos, assignedResourceType);
        if (nearest != null && nearest.CanMine())
        {
            targetSource = nearest;
            npc.MoveTo(((MonoBehaviour)targetSource).transform.position);
            state = State.MovingToSource;
            animController?.StopMining();
            return;
        }

        IResourceSource fallback = FindNearestResourceSourceOfType(((MonoBehaviour)npc).transform.position, assignedResourceType);
        if (fallback != null && fallback.CanMine())
        {
            targetSource = fallback;
            npc.MoveTo(((MonoBehaviour)targetSource).transform.position);
            state = State.MovingToSource;
            animController?.StopMining();
            return;
        }

        ClearAssignment();
    }

    private void FindResourceNode()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius);
        IResourceSource best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var rs = c.GetComponent<IResourceSource>();
            if (rs == null || !rs.CanMine()) continue;

            float d = Vector3.Distance(transform.position, ((MonoBehaviour)rs).transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = rs;
            }
        }

        if (best != null)
        {
            targetSource = best;
            npc.MoveTo(((MonoBehaviour)targetSource).transform.position);
            state = State.MovingToSource;
            animController?.StopMining();
        }
    }

    private IResourceSource FindNearestResourceSourceOfType(Vector3 origin, ItemType type)
    {
        Collider[] cols = Physics.OverlapSphere(origin, searchRadius);
        IResourceSource best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var rs = c.GetComponent<IResourceSource>();
            if (rs == null || rs.Type != type || !rs.CanMine()) continue;

            float d = Vector3.Distance(origin, ((MonoBehaviour)rs).transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = rs;
            }
        }

        return best;
    }

    #endregion

    #region UI Updates

    private void UpdateUiStatus(State s)
    {
        switch (s)
        {
            case State.Idle:
                npc?.SetUiStatus(hasAssignedSourcePos ? "Waiting for task" : "Idle");
                break;
            case State.MovingToSource:
                npc?.SetUiStatus($"Going to mine {assignedResourceType}");
                break;
            case State.Mining:
                npc?.SetUiStatus($"Mining {assignedResourceType}");
                break;
        }
    }

    #endregion

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRadius);
    }
}