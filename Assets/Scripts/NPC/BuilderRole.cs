using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BuilderRole : NPCRoleBase
{
    [Header("References")]
    private NPCAnimationController animController;
    private AudioSource audioSource;

    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;

    [Header("Building Settings")]
    [SerializeField] private int carryCapacity = 5;
    [SerializeField] private float depositInterval = 5f;
    [SerializeField] private int depositAmountPerInterval = 1;

    [Header("Robustness")]
    [SerializeField] private float holderArrivalThreshold = 1.0f;

    [Header("Waiting")]
    [SerializeField] private float waitCheckInterval = 5f;

    [Header("Animation")]
    [SerializeField] private float buildingAnimationSpeed = 1f;
    [SerializeField] private float buildingSoundInterval = 1f;

    private IBuildingSite assignedBuilding;
    private IBuildingSite targetBuilding;

    private List<(ItemType type, int amount)> inventory = new();

    private enum State
    {
        Idle,
        MovingToStorage,
        TakingFromStorage,
        WaitingAtHolder,
        MovingToBuilding,
        Building
    }

    private State state = State.Idle;
    private State previousState = State.Idle;

    private float buildTimer = 0f;
    private float stateTimer = 0f;
    private float waitTimer = 0f;
    private float soundTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
        animController = GetComponent<NPCAnimationController>();
        audioSource = GetComponent<AudioSource>();

        if (animController == null)
        {
            animController = gameObject.AddComponent<NPCAnimationController>();
            Debug.LogWarning($"BuilderRole: Added NPCAnimationController to {gameObject.name}");
        }
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        stateTimer += dt;
        waitTimer += dt;
        soundTimer += dt;

        switch (state)
        {
            case State.Idle:
                if (assignedBuilding != null)
                    StartAssignedBuildingWork();
                else if (autoSearch)
                    FindBuilding();
                break;

            case State.MovingToStorage:
                if (targetBuilding == null)
                {
                    TransitionToIdle();
                    break;
                }

                if (IsAtHolderPosition())
                {
                    TakeResourcesFromStorage();

                    if (GetInventoryTotal() > 0)
                    {
                        npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                        state = State.MovingToBuilding;
                        ResetStateTimer();
                    }
                    else
                    {
                        if (!BuildingNeedsAny(targetBuilding))
                        {
                            TransitionToIdle();
                            break;
                        }

                        TransitionToWaitingAtHolder();
                    }
                    break;
                }

                if (npc.IsAtDestination())
                {
                    TakeResourcesFromStorage();

                    if (GetInventoryTotal() > 0)
                    {
                        npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                        state = State.MovingToBuilding;
                        ResetStateTimer();
                    }
                    else
                    {
                        if (!BuildingNeedsAny(targetBuilding))
                        {
                            TransitionToIdle();
                            break;
                        }

                        TransitionToWaitingAtHolder();
                    }
                }
                break;

            case State.TakingFromStorage:
                if (targetBuilding == null)
                {
                    TransitionToIdle();
                    break;
                }

                TakeResourcesFromStorage();

                if (GetInventoryTotal() > 0)
                {
                    npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                    state = State.MovingToBuilding;
                    ResetStateTimer();
                }
                else
                {
                    if (!BuildingNeedsAny(targetBuilding))
                    {
                        TransitionToIdle();
                        break;
                    }

                    TransitionToWaitingAtHolder();
                }
                break;

            case State.WaitingAtHolder:
                if (targetBuilding == null)
                {
                    TransitionToIdle();
                    break;
                }

                if (!BuildingNeedsAny(targetBuilding))
                {
                    TransitionToIdle();
                    break;
                }

                if (waitTimer >= waitCheckInterval)
                {
                    ResetWaitTimer();
                    TakeResourcesFromStorage();

                    if (GetInventoryTotal() > 0)
                    {
                        npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                        state = State.MovingToBuilding;
                        ResetStateTimer();
                    }
                }
                break;

            case State.MovingToBuilding:
                if (targetBuilding == null)
                {
                    TransitionToIdle();
                    break;
                }

                if (npc.IsAtDestination())
                    TransitionToBuilding();
                break;

            case State.Building:
                if (targetBuilding == null)
                {
                    TransitionToIdle();
                    break;
                }

                // Look at building while working
                npc.LookAtTarget(((MonoBehaviour)targetBuilding).transform.position);

                // Play building sounds periodically
                if (soundTimer >= buildingSoundInterval)
                {
                    soundTimer = 0f;
                    PlayBuildingSound();
                }

                buildTimer += dt;

                if (buildTimer >= depositInterval)
                {
                    buildTimer = 0f;

                    if (DepositOneResource())
                    {
                        // Successfully deposited, continue
                    }
                    else
                    {
                        // Inventory empty
                        if (BuildingNeedsAny(targetBuilding))
                        {
                            npc.MoveTo(targetBuilding.GetHolderPosition());
                            state = State.MovingToStorage;
                            animController?.StopBuilding();
                            ResetStateTimer();
                        }
                        else
                        {
                            TransitionToIdle();
                        }
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

        // Safety timeout
        if (stateTimer > 60f)
        {
            Debug.LogWarning($"Builder ({name}): state {state} stuck too long, resetting to Idle.");
            ClearAssignment();
        }
    }

    #region State Transitions

    private void TransitionToIdle()
    {
        state = State.Idle;
        npc.Stop();
        animController?.StopBuilding();
        ResetStateTimer();
    }

    private void TransitionToWaitingAtHolder()
    {
        state = State.WaitingAtHolder;
        npc.Stop();
        animController?.StopBuilding();
        ResetStateTimer();
        ResetWaitTimer();
    }

    private void TransitionToBuilding()
    {
        state = State.Building;
        buildTimer = 0f;
        soundTimer = 0f;
        npc.Stop();
        animController?.StartBuilding(buildingAnimationSpeed);
        ResetStateTimer();
    }

    private void ResetStateTimer() => stateTimer = 0f;
    private void ResetWaitTimer() => waitTimer = 0f;

    #endregion

    #region Building Logic

    private bool DepositOneResource()
    {
        if (inventory.Count == 0) return false;
        if (targetBuilding == null) return false;

        var buildingSite = targetBuilding as BuildingSite;
        if (buildingSite == null) return false;

        var item = inventory[0];
        buildingSite.AddResourceByBuilder(item.type, depositAmountPerInterval);

        if ((item.amount - depositAmountPerInterval) <= 1)
            inventory.RemoveAt(0);
        else
            inventory[0] = (item.type, item.amount - depositAmountPerInterval);

        UpdateUiStatus(state);
        return true;
    }

    private void PlayBuildingSound()
    {
        if (audioSource != null)
        {
            SoundManager.Instance.PlayOnSourceWithoutInterrupt(audioSource, SoundSO.Sound.Builder_Building);
        }
    }

    #endregion

    #region Storage Management

    private void TakeResourcesFromStorage()
    {
        if (targetBuilding == null) return;

        var buildingSite = targetBuilding as BuildingSite;
        if (buildingSite == null || buildingSite.resourceHolder == null) return;

        var holder = buildingSite.resourceHolder;

        ItemType? neededType = GetMostNeededType(buildingSite);
        if (!neededType.HasValue) return;

        int available = holder.GetResourceCount(neededType.Value);
        int toTake = Mathf.Min(carryCapacity, available);

        if (toTake > 0)
        {
            int removed = holder.RemoveResource(neededType.Value, toTake);
            inventory.Add((neededType.Value, removed));
        }
        else
        {
            ItemType holderType = holder.GetItemInHolder();
            available = holder.GetResourceCount(holderType);
            toTake = Mathf.Min(carryCapacity, available);

            if (toTake > 0 && NeedsResource(buildingSite, holderType))
            {
                int removed = holder.RemoveResource(holderType, toTake);
                inventory.Add((holderType, removed));
            }
        }
    }

    #endregion

    #region Assignment API

    public void AssignBuildingSite(IBuildingSite site)
    {
        if (site == null) return;

        assignedBuilding = site;
        targetBuilding = site;

        if (IsAtHolderPosition())
        {
            TakeResourcesFromStorage();

            if (GetInventoryTotal() > 0)
            {
                npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                state = State.MovingToBuilding;
                ResetStateTimer();
            }
            else
            {
                if (!BuildingNeedsAny(targetBuilding))
                    TransitionToIdle();
                else
                    TransitionToWaitingAtHolder();
            }
        }
        else
        {
            npc.MoveTo(targetBuilding.GetHolderPosition());
            state = State.MovingToStorage;
            ResetStateTimer();
        }
    }

    public void ClearAssignment()
    {
        assignedBuilding = null;
        targetBuilding = null;
        inventory.Clear();
        TransitionToIdle();
    }

    #endregion

    #region Search Logic

    private void StartAssignedBuildingWork()
    {
        if (assignedBuilding == null) return;
        targetBuilding = assignedBuilding;

        if (IsAtHolderPosition())
        {
            TakeResourcesFromStorage();

            if (GetInventoryTotal() > 0)
            {
                npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                state = State.MovingToBuilding;
                ResetStateTimer();
            }
            else
            {
                if (!BuildingNeedsAny(targetBuilding))
                    TransitionToIdle();
                else
                    TransitionToWaitingAtHolder();
            }
            return;
        }

        npc.MoveTo(targetBuilding.GetHolderPosition());
        state = State.MovingToStorage;
        ResetStateTimer();
    }

    private void FindBuilding()
    {
        Collider[] cols = Physics.OverlapSphere(transform.position, searchRadius);
        IBuildingSite best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cols)
        {
            var bs = c.GetComponent<IBuildingSite>();
            if (bs == null || !BuildingNeedsAny(bs)) continue;

            float d = Vector3.Distance(transform.position, ((MonoBehaviour)bs).transform.position);
            if (d < bestDist)
            {
                bestDist = d;
                best = bs;
            }
        }

        if (best != null)
        {
            targetBuilding = best;
            npc.MoveTo(targetBuilding.GetHolderPosition());
            state = State.MovingToStorage;
            ResetStateTimer();
        }
    }

    #endregion

    #region Helper Methods

    private ItemType? GetMostNeededType(IBuildingSite building)
    {
        if (building == null) return null;

        if (building.NeedsResourceForBuilders(ItemType.Stone)) return ItemType.Stone;
        if (building.NeedsResourceForBuilders(ItemType.Wood)) return ItemType.Wood;
        if (building.NeedsResourceForBuilders(ItemType.Ore)) return ItemType.Ore;

        return null;
    }

    private bool BuildingNeedsAny(IBuildingSite building)
    {
        if (building == null) return false;

        return building.NeedsResourceForBuilders(ItemType.Wood) ||
               building.NeedsResourceForBuilders(ItemType.Stone) ||
               building.NeedsResourceForBuilders(ItemType.Ore);
    }

    private bool NeedsResource(IBuildingSite building, ItemType type)
    {
        if (building == null) return false;
        return building.NeedsResourceForBuilders(type);
    }

    private int GetInventoryTotal()
    {
        int total = 0;
        foreach (var item in inventory)
            total += item.amount;
        return total;
    }

    private bool IsAtHolderPosition()
    {
        if (targetBuilding == null) return false;
        var holderPos = targetBuilding.GetHolderPosition();
        return Vector3.Distance(((MonoBehaviour)npc).transform.position, holderPos) <= holderArrivalThreshold;
    }

    #endregion

    #region UI Updates

    private void UpdateUiStatus(State s)
    {
        switch (s)
        {
            case State.Idle:
                npc?.SetUiStatus(assignedBuilding != null ? "Ready to work" : "Idle");
                break;
            case State.MovingToStorage:
                npc?.SetUiStatus("Going to storage");
                break;
            case State.TakingFromStorage:
                npc?.SetUiStatus("Taking materials");
                break;
            case State.WaitingAtHolder:
                npc?.SetUiStatus("Waiting at holder");
                break;
            case State.MovingToBuilding:
                npc?.SetUiStatus($"Carrying to building ({GetInventoryTotal()}x)");
                break;
            case State.Building:
                npc?.SetUiStatus($"Building ({GetInventoryTotal()}x left)");
                break;
        }
    }

    #endregion
}