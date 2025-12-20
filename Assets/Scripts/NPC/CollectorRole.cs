using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CollectorRole : NPCRoleBase
{
    [Header("References")]
    private NPCAnimationController animController;

    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;
    public int capacity = 5;

    [Header("Waiting")]
    [SerializeField] private float waitCheckInterval = 5f;

    [Header("Animation")]
    [SerializeField] private bool useCarryingAnimation = true;

    private List<(ItemType type, int amount)> inventory = new();
    private IGroundItem targetItem;
    private IBuildingSite targetBuilding;

    private ItemType? lastTargetType = null;

    // External assignment
    private Vector3 assignedSourcePos;
    private bool hasAssignedSourcePos = false;
    private IBuildingSite assignedDestination;

    private enum State { Idle, MovingToItem, PickingUp, WaitingAtSource, MovingToBuilding, Depositing }
    private State state = State.Idle;
    private State previousState = State.Idle;

    private float waitTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        animController = GetComponent<NPCAnimationController>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<NPCAnimationController>();
            Debug.LogWarning($"CollectorRole: Added NPCAnimationController to {gameObject.name}");
        }
    }

    private void Update()
    {
        if (assignedDestination != null && !BuildingNeedsAny(assignedDestination))
            ClearAssignment();

        waitTimer += Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                UpdateCarryingAnimation();

                if (hasAssignedSourcePos && assignedDestination != null)
                    StartAssignedCollection();
                else if (inventoryTotal() < capacity && autoSearch)
                    FindGroundItem();
                else if (inventoryTotal() >= capacity && autoSearch)
                    FindBuildingAndDeliver();
                break;

            case State.MovingToItem:
                UpdateCarryingAnimation();

                if (targetItem == null)
                {
                    TransitionToIdle();
                    break;
                }

                if (npc.IsAtDestination())
                    TransitionToPickingUp();
                break;

            case State.PickingUp:
                if (targetItem == null)
                {
                    TransitionToIdle();
                    break;
                }

                // 🔹 VIZUÁLNÍ ANIMACE (NEBLOKUJE LOGIKU)
                animController?.PlayPickup();

                // 🔹 OKAMŽITÝ PICKUP
                OnPickUp((GroundItem)((MonoBehaviour)targetItem));
                UpdateCarryingAnimation();

                // 🔹 rozhodnutí co dál
                if (inventoryTotal() >= capacity)
                {
                    targetBuilding = assignedDestination != null
                        ? assignedDestination
                        : FindBuildingAndDeliverFallback();

                    if (targetBuilding != null)
                    {
                        npc.MoveTo(targetBuilding.GetHolderPosition());
                        state = State.MovingToBuilding;
                        break;
                    }
                }

                if (assignedDestination != null && BuildingNeedsAny(assignedDestination))
                {
                    targetItem = null;
                    lastTargetType = null;
                    StartAssignedCollection();
                    break;
                }

                targetItem = null;
                lastTargetType = null;
                TransitionToIdle();
                break;

            case State.WaitingAtSource:
                UpdateCarryingAnimation();

                if (!BuildingNeedsAny(assignedDestination))
                {
                    ClearAssignment();
                    break;
                }

                if (waitTimer >= waitCheckInterval)
                {
                    waitTimer = 0f;
                    var neededTypes = GetNeededTypesOrdered(assignedDestination);

                    foreach (var t in neededTypes)
                    {
                        var found = FindNearestGroundItemOfType(assignedSourcePos, t);
                        if (found != null)
                        {
                            targetItem = found;
                            lastTargetType = t;
                            npc.MoveTo(((MonoBehaviour)found).transform.position);
                            state = State.MovingToItem;
                            break;
                        }
                    }
                }
                break;

            case State.MovingToBuilding:
                UpdateCarryingAnimation();

                if (targetBuilding == null)
                {
                    TransitionToIdle();
                    break;
                }

                if (npc.IsAtDestination())
                    TransitionToDepositing();
                break;

            case State.Depositing:
                if (targetBuilding != null)
                {
                    DepositAll();

                    if (hasAssignedSourcePos && assignedDestination != null && BuildingNeedsAny(assignedDestination))
                        StartAssignedCollection();
                    else if (hasAssignedSourcePos)
                        ClearAssignment();
                }

                TransitionToIdle();
                break;
        }

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
    }

    private void TransitionToPickingUp()
    {
        state = State.PickingUp;
        npc.Stop();
    }

    private void TransitionToDepositing()
    {
        state = State.Depositing;
        npc.Stop();
    }

    #endregion

    #region Animation

    private void UpdateCarryingAnimation()
    {
        if (!useCarryingAnimation || animController == null) return;
        animController.SetCarrying(inventoryTotal() > 0);
    }

    #endregion

    #region Pickup & Deposit

    public void OnPickUp(GroundItem gi)
    {
        if (gi == null || inventoryTotal() >= capacity) return;

        gi.PickUp(transform);
        inventory.Add((gi.Type, gi.Quantity));
        lastTargetType = gi.Type;
    }

    private void DepositAll()
    {
        if (targetBuilding == null) return;

        var delivered = new List<(ItemType type, int amount)>();
        foreach (var it in inventory)
        {
            if (targetBuilding.NeedsResourceForCollectors(it.type))
            {
                targetBuilding.AddResourceByCollector(it.type, it.amount);
                delivered.Add(it);
            }
        }

        foreach (var d in delivered)
            inventory.RemoveAll(i => i.type == d.type && i.amount == d.amount);

        UpdateCarryingAnimation();
    }

    #endregion

    #region Assignment API

    public void AssignCollectionTask(Vector3 sourcePos, IBuildingSite deliverySite)
    {
        if (deliverySite == null) return;

        assignedSourcePos = sourcePos;
        hasAssignedSourcePos = true;
        assignedDestination = deliverySite;

        inventory.RemoveAll(it => !assignedDestination.NeedsResourceForCollectors(it.type));
        UpdateCarryingAnimation();

        if (BuildingNeedsAny(assignedDestination))
            StartAssignedCollection();
        else
            ClearAssignment();
    }

    public void ClearAssignment()
    {
        hasAssignedSourcePos = false;
        assignedDestination = null;
        targetItem = null;
        targetBuilding = null;
        lastTargetType = null;
        inventory.Clear();
        UpdateCarryingAnimation();
        TransitionToIdle();
    }

    #endregion

    #region Search Logic

    private void StartAssignedCollection()
    {
        if (!hasAssignedSourcePos || assignedDestination == null) return;

        var neededTypes = GetNeededTypesOrdered(assignedDestination);
        if (neededTypes == null || neededTypes.Count == 0) return;

        IGroundItem nearest = null;
        foreach (var t in neededTypes)
        {
            nearest = FindNearestGroundItemOfType(assignedSourcePos, t);
            if (nearest != null) break;
        }

        if (nearest != null)
        {
            targetItem = nearest;
            lastTargetType = TryGetTypeFromGroundItem(nearest);
            npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
            state = State.MovingToItem;
            return;
        }

        npc.MoveTo(assignedSourcePos);
        state = State.WaitingAtSource;
        waitTimer = 0f;
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

            float d = Vector3.Distance(transform.position, ((MonoBehaviour)gi).transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = gi;
            }
        }

        if (best != null)
        {
            targetItem = best;
            lastTargetType = TryGetTypeFromGroundItem(best);
            npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
            state = State.MovingToItem;
        }
    }

    private void FindBuildingAndDeliver()
    {
        var best = FindBuildingAndDeliverFallback();
        if (best != null)
        {
            targetBuilding = best;
            npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
            state = State.MovingToBuilding;
        }
    }

    private IBuildingSite FindBuildingAndDeliverFallback()
    {
        Collider[] cols = Physics.OverlapSphere(((MonoBehaviour)npc).transform.position, searchRadius);
        IBuildingSite best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var bs = c.GetComponent<IBuildingSite>();
            if (bs == null) continue;

            foreach (var it in inventory)
            {
                if (bs.NeedsResourceForCollectors(it.type))
                {
                    float d = Vector3.Distance(((MonoBehaviour)npc).transform.position, ((MonoBehaviour)bs).transform.position);
                    if (d < bestDist)
                    {
                        bestDist = d;
                        best = bs;
                    }
                    break;
                }
            }
        }

        return best;
    }

    private IGroundItem FindNearestGroundItemOfType(Vector3 origin, ItemType type)
    {
        Collider[] cols = Physics.OverlapSphere(origin, searchRadius);
        IGroundItem best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var gi = c.GetComponent<IGroundItem>();
            if (gi == null || gi.Type != type) continue;

            float d = Vector3.Distance(origin, ((MonoBehaviour)gi).transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = gi;
            }
        }

        return best;
    }

    #endregion

    #region Helper Methods

    private List<ItemType> GetNeededTypesOrdered(IBuildingSite b)
    {
        var list = new List<ItemType>();
        if (b == null) return list;

        if (b.NeedsResourceForCollectors(ItemType.Wood)) list.Add(ItemType.Wood);
        if (b.NeedsResourceForCollectors(ItemType.Stone)) list.Add(ItemType.Stone);
        if (b.NeedsResourceForCollectors(ItemType.Ore)) list.Add(ItemType.Ore);

        return list;
    }

    private bool BuildingNeedsAny(IBuildingSite b)
    {
        if (b == null) return false;
        return b.NeedsResourceForCollectors(ItemType.Wood) ||
               b.NeedsResourceForCollectors(ItemType.Stone) ||
               b.NeedsResourceForCollectors(ItemType.Ore);
    }

    private int inventoryTotal()
    {
        int s = 0;
        foreach (var it in inventory) s += it.amount;
        return s;
    }

    private ItemType? TryGetTypeFromGroundItem(IGroundItem gi)
    {
        if (gi == null) return null;
        try { return gi.Type; }
        catch { return null; }
    }

    #endregion

    #region UI Updates

    private void UpdateUiStatus(State s)
    {
        switch (s)
        {
            case State.Idle:
                npc?.SetUiStatus(hasAssignedSourcePos && assignedDestination != null ? "Waiting for task" : "Idle");
                break;
            case State.MovingToItem:
                string t = lastTargetType?.ToString() ?? "resource";
                npc?.SetUiStatus($"Fetching {t}");
                break;
            case State.PickingUp:
                string t2 = lastTargetType?.ToString() ?? "resource";
                npc?.SetUiStatus($"Collecting {t2}");
                break;
            case State.WaitingAtSource:
                npc?.SetUiStatus("Waiting for resources");
                break;
            case State.MovingToBuilding:
                npc?.SetUiStatus("Delivering");
                break;
            case State.Depositing:
                npc?.SetUiStatus("Depositing");
                break;
        }
    }

    #endregion
}