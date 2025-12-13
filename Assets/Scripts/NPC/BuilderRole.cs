using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BuilderRole : NPCRoleBase
{
    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;

    [Header("Building Settings")]
    [SerializeField] private int carryCapacity = 5; // Kolik kusù mùže nést najednou
    [SerializeField] private float depositInterval = 5f; // Jak èasto vkládá jeden kus (v sekundách)

    [Header("Robustness")]
    [SerializeField, Tooltip("Distance threshold to treat builder as 'arrived' at holder position")] private float holderArrivalThreshold = 1.0f;

    // Externí pøiøazení building site (èeká na povel)
    private IBuildingSite assignedBuilding;
    private IBuildingSite targetBuilding;

    // Inventáø buildera - uchovává co právì nese
    private List<(ItemType type, int amount)> inventory = new();

    private enum State
    {
        Idle,
        MovingToStorage,      // Jde pro materiál
        TakingFromStorage,    // Bere ze storage
        MovingToBuilding,     // Nese k budovì
        Building              // Vkládá postupnì do budovy
    }

    private State state = State.Idle;
    private State previousState = State.Idle;

    private float buildTimer = 0f;
    private float stateTimer = 0f;

    void Update()
    {
        // update timer
        stateTimer += Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                if (assignedBuilding != null)
                {
                    StartAssignedBuildingWork();
                }
                else if (autoSearch)
                {
                    FindBuilding();
                }
                break;

            case State.MovingToStorage:
                if (targetBuilding == null) { state = State.Idle; ResetStateTimer(); break; }

                // pokud jsme už u holderu (napø. dostali pøíkaz znovu), pøejdeme rovnou do TakingFromStorage
                if (IsAtHolderPosition())
                {
                    Debug.Log($"Builder ({name}): already at holder -> start taking");
                    state = State.TakingFromStorage;
                    ResetStateTimer();
                    npc.Stop();
                    break;
                }

                if (npc.IsAtDestination())
                {
                    state = State.TakingFromStorage;
                    npc.Stop();
                    ResetStateTimer();
                }
                break;

            case State.TakingFromStorage:
                if (targetBuilding == null) { state = State.Idle; ResetStateTimer(); break; }

                TakeResourcesFromStorage();

                // Pokud jsme vzali nìco, jdeme k budovì
                if (GetInventoryTotal() > 0)
                {
                    npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                    state = State.MovingToBuilding;
                    ResetStateTimer();
                }
                else
                {
                    // Nic není ve storage, èekáme nebo konèíme
                    Debug.Log($"Builder ({name}): nothing available in storage for {targetBuilding}. going idle for now.");
                    state = State.Idle;
                    ResetStateTimer();
                }
                break;

            case State.MovingToBuilding:
                if (targetBuilding == null) { state = State.Idle; ResetStateTimer(); break; }

                if (npc.IsAtDestination())
                {
                    state = State.Building;
                    buildTimer = 0f;
                    npc.Stop();
                    ResetStateTimer();
                }
                break;

            case State.Building:
                if (targetBuilding == null) { state = State.Idle; ResetStateTimer(); break; }

                // Postupnì vkládáme jeden kus každých X vteøin
                buildTimer += Time.deltaTime;

                if (buildTimer >= depositInterval)
                {
                    buildTimer = 0f;

                    if (DepositOneResource())
                    {
                        // Úspìšnì vloženo, pokraèujeme (èekáme na další interval nebo dojde-li inventáø)
                        Debug.Log($"Builder ({name}): deposited one. remaining inventory {GetInventoryTotal()}");
                    }
                    else
                    {
                        // Inventáø prázdný, zjistíme, zda budova ještì potøebuje
                        if (BuildingNeedsAny(targetBuilding))
                        {
                            // Pokud jsme již u holderu, voláme okamžitì TakeResourcesFromStorage,
                            // jinak se pøesuneme k holderu.
                            if (IsAtHolderPosition())
                            {
                                Debug.Log($"Builder ({name}): at holder position, taking resources immediately.");
                                state = State.TakingFromStorage;
                                ResetStateTimer();
                                TakeResourcesFromStorage();
                                if (GetInventoryTotal() > 0)
                                {
                                    npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                                    state = State.MovingToBuilding;
                                    ResetStateTimer();
                                }
                            }
                            else
                            {
                                npc.MoveTo(targetBuilding.GetHolderPosition());
                                state = State.MovingToStorage;
                                ResetStateTimer();
                            }
                        }
                        else
                        {
                            // Budova už nic nepotøebuje
                            Debug.Log($"Builder ({name}): building no longer needs resources -> idle");
                            state = State.Idle;
                            ResetStateTimer();
                        }
                    }
                }
                break;
        }

        // Aktualizuj UI status jen pøi zmìnì stavu
        if (state != previousState)
        {
            UpdateUiStatus(state);
            previousState = state;
        }

        // bezpeènostní timeout: pokud jsme "stuck" velmi dlouho, pøejdeme do Idle a resetneme cíle
        if (stateTimer > 60f)
        {
            Debug.LogWarning($"Builder ({name}): state {state} stuck too long, resetting to Idle.");
            ClearAssignment();
        }
    }

    private void ResetStateTimer()
    {
        stateTimer = 0f;
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
            case State.MovingToStorage:
                npc?.SetUiStatus("Jdu pro materiál");
                break;
            case State.TakingFromStorage:
                npc?.SetUiStatus("Beru materiál");
                break;
            case State.MovingToBuilding:
                npc?.SetUiStatus($"Nesu k budovì ({GetInventoryTotal()}x)");
                break;
            case State.Building:
                npc?.SetUiStatus($"Stavím ({GetInventoryTotal()}x zbývá)");
                break;
        }
    }

    // Externí API: pøiøaï building site a okamžitì ho zaèni obsluhovat
    public void AssignBuildingSite(IBuildingSite site)
    {
        if (site == null) return;
        assignedBuilding = site;
        targetBuilding = site;

        // Pokud jsme již u holderu, rovnou zaèni brát
        if (IsAtHolderPosition())
        {
            state = State.TakingFromStorage;
            ResetStateTimer();
            Debug.Log($"Builder ({name}): assigned and already at holder -> taking immediately");
            TakeResourcesFromStorage();
            if (GetInventoryTotal() > 0)
            {
                npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                state = State.MovingToBuilding;
                ResetStateTimer();
            }
        }
        else
        {
            // Zaèneme tím, že jdeme pro materiál
            npc.MoveTo(targetBuilding.GetHolderPosition());
            state = State.MovingToStorage;
            ResetStateTimer();
        }
    }

    // Externí API: zruší pøiøazení a vrátí roli do idle stavu
    public void ClearAssignment()
    {
        assignedBuilding = null;
        targetBuilding = null;
        inventory.Clear();
        state = State.Idle;
        npc.Stop();
        ResetStateTimer();
    }

    private void StartAssignedBuildingWork()
    {
        if (assignedBuilding == null) return;
        targetBuilding = assignedBuilding;

        // Pokud jsme již u holderu, pøímo ber
        if (IsAtHolderPosition())
        {
            state = State.TakingFromStorage;
            ResetStateTimer();
            TakeResourcesFromStorage();
            if (GetInventoryTotal() > 0)
            {
                npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                state = State.MovingToBuilding;
                ResetStateTimer();
            }
            return;
        }

        // Jdeme pro materiál ze storage
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
            if (bs == null) continue;

            // Pokud budova potøebuje nìjaké zdroje
            if (BuildingNeedsAny(bs))
            {
                float d = Vector3.Distance(transform.position, ((MonoBehaviour)bs).transform.position);
                if (d < bestDist) { bestDist = d; best = bs; }
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

    private void TakeResourcesFromStorage()
    {
        if (targetBuilding == null) return;

        var buildingSite = targetBuilding as BuildingSite;
        if (buildingSite == null || buildingSite.resourceHolder == null) return;

        var holder = buildingSite.resourceHolder;

        // Zjistíme, jaký typ zdroje budova nejvíce potøebuje
        ItemType? neededType = GetMostNeededType(buildingSite);
        if (!neededType.HasValue)
        {
            Debug.Log($"Builder ({name}): building does not need any resources.");
            return;
        }

        // Zjistíme, kolik mùžeme vzít
        int available = holder.GetResourceCount(neededType.Value);
        int toTake = Mathf.Min(carryCapacity, available);


        if (toTake > 0)
        {
            // Odebereme ze storage
            int removed = holder.RemoveResource(neededType.Value, toTake);

            // Pøidáme do inventáøe
            inventory.Add((neededType.Value, removed));
            
            Debug.Log($"Builder ({name}) vzal {removed}x {neededType.Value} ze storage.");
        } else
        {
            ItemType holderType;
            // Pokud není dostupný preferovaný typ, zkusíme vzít jakýkoli jiný, který budova potøebuje

            holderType = holder.GetItemInHolder();
            available = holder.GetResourceCount(holderType);
            toTake = Mathf.Min(carryCapacity, available);

            if (toTake > 0 && NeedsResource(buildingSite, holderType))
            {
                int removed = holder.RemoveResource(holderType, toTake);
                inventory.Add((holderType, removed));
                Debug.Log($"Builder ({name}) vzal {removed}x {holderType} ze storage (alternativa).");
            }
            else            {
                Debug.Log($"Builder ({name}): nothing available to take from storage.");
            }
        }
      

        

       
       
    }

    private bool DepositOneResource()
    {
        if (inventory.Count == 0) return false;
        if (targetBuilding == null) return false;

        var buildingSite = targetBuilding as BuildingSite;
        if (buildingSite == null) return false;

        // Vezmeme první dostupný typ z inventáøe
        var item = inventory[0];

        // Vložíme jeden kus do budovy
        buildingSite.AddResourceByBuilder(item.type, 1);

        // Aktualizujeme inventáø
        if (item.amount <= 1)
        {
            inventory.RemoveAt(0);
        }
        else
        {
            inventory[0] = (item.type, item.amount - 1);
        }
        UpdateUiStatus(state);
        Debug.Log($"Builder ({name}) vložil 1x {item.type} do budovy. Zbývá: {GetInventoryTotal()}");

        return true;
    }

    private ItemType? GetMostNeededType(IBuildingSite building)
    {
        if (building == null) return null;

        // Priorita: Stone -> Wood -> Ore
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
        {
            total += item.amount;
        }
        return total;
    }

    private bool IsAtHolderPosition()
    {
        if (targetBuilding == null) return false;
        var holderPos = targetBuilding.GetHolderPosition();
        return Vector3.Distance(((MonoBehaviour)npc).transform.position, holderPos) <= holderArrivalThreshold;
    }

}