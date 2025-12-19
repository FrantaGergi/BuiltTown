using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CollectorRole : NPCRoleBase
{
    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;
    public int capacity = 5;

    [Header("Waiting (assigned audioSource)")]
    [SerializeField, Tooltip("Kontrolní interval (s) když stojíme na assignedSourcePos a hledáme položky")] 
    private float waitCheckInterval = 5f;

    private List<(ItemType type, int amount)> inventory = new();
    private IGroundItem targetItem;
    private IBuildingSite targetBuilding;

    // uložíme poslední známý typ targetItemu (bez volání GetComponent na možné zničený objekt)
    private ItemType? lastTargetType = null;

    // Externí přiřazení úkolu: souřadnice odkud hledat + kam doručit
    private Vector3 assignedSourcePos;
    private bool hasAssignedSourcePos = false;
    private IBuildingSite assignedDestination;

    private enum State { Idle, MovingToItem, PickingUp, WaitingAtSource, MovingToBuilding, Depositing }
    private State state = State.Idle;
    private State previousState = State.Idle;

    private float waitTimer = 0f;

    void Update()
    {
        // Pokud je budova dokončena, vyčistit assignment a jít do idle
        if (assignedDestination != null && !BuildingNeedsAny(assignedDestination))
        {
            ClearAssignment(); // včetně vymazání přidělené sourcePos
        }

        // akumulátor pro waiting interval
        waitTimer += Time.deltaTime;

        switch (state)
        {
            case State.Idle:
                // Pokud máme explicitní úkol, spusť ho
                if (hasAssignedSourcePos && assignedDestination != null)
                {
                    StartAssignedCollection();
                }
                else if (inventoryTotal() < capacity)
                {
                    if (autoSearch)
                        FindGroundItem();
                }
                else
                {
                    if (autoSearch)
                        FindBuildingAndDeliver();
                }
                break;

            case State.MovingToItem:
                if (targetItem == null) { state = State.Idle; break; }
                if (npc.IsAtDestination()) { state = State.PickingUp; npc.Stop(); }
                break;

            case State.PickingUp:
                if (targetItem == null) { state = State.Idle; break; }
                // pick up
                OnPickUp((GroundItem)((MonoBehaviour)targetItem));
                // Po pickupu rozhodneme, kam jít dál v závislosti na potřebě budovy
                // 1) Kapacita plná -> okamžitě doruč
                if (inventoryTotal() >= capacity)
                {
                    if (assignedDestination != null)
                        targetBuilding = assignedDestination;
                    else
                        targetBuilding = FindBuildingAndDeliverFallback();

                    if (targetBuilding != null)
                    {
                        npc.MoveTo(targetBuilding.GetHolderPosition());
                        state = State.MovingToBuilding;
                        return;
                    }
                }

                // 2) Máme úkol -> budova stále potřebuje -> pokračuj v hledání dalšího itemu
                if (assignedDestination != null && BuildingNeedsAny(assignedDestination))
                {
                    targetItem = null;
                    lastTargetType = null;
                    StartAssignedCollection(); // on najde další item správného typu (nebo pošle doručení)
                    return;
                }

                // 3) AutoSearch verze
                targetItem = null;
                lastTargetType = null;
                state = State.Idle;
                break;

            case State.WaitingAtSource:
                // pokud budova už nic nepotřebuje => ukonči
                if (!BuildingNeedsAny(assignedDestination))
                {
                    ClearAssignment();
                    break;
                }

                // kontroluj pouze periodicky podle waitCheckInterval
                if (waitTimer >= waitCheckInterval)
                {
                    waitTimer = 0f;
                    // zkus znovu najít nějaký item v assignedSourcePos dle priority
                    var neededTypes = GetNeededTypesOrdered(assignedDestination);
                    IGroundItem found = null;
                    ItemType? foundType = null;
                    foreach (var t in neededTypes)
                    {
                        found = FindNearestGroundItemOfType(assignedSourcePos, t);
                        if (found != null) { foundType = t; break; }
                    }

                    if (found != null && foundType.HasValue)
                    {
                        targetItem = found;
                        lastTargetType = TryGetTypeFromGroundItem(found);
                        npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
                        state = State.MovingToItem;
                        return;
                    }
                    // jinak zůstaneme čekat až další interval
                }
                break;

            case State.MovingToBuilding:
                if (targetBuilding == null) { state = State.Idle; break; }
                if (npc.IsAtDestination()) { state = State.Depositing; npc.Stop(); }
                break;

            case State.Depositing:
                if (targetBuilding == null) { state = State.Idle; break; }
                DepositAll();
                // pokud to byl explicitní úkol, po dodání znovu zkontrolujeme další potřeby
                if (hasAssignedSourcePos && assignedDestination != null && BuildingNeedsAny(assignedDestination))
                {
                    // začneme znovu hledat další resource od assignedSourcePos
                    StartAssignedCollection();
                }
                else
                {
                    // pokud úkol dokončen nebo žádná další potřeba, vyčistit assignment
                    if (hasAssignedSourcePos)
                        ClearAssignment();
                }
                state = State.Idle;
                break;
        }

        // Aktualizuj UI status jen při změně stavu
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
                if (hasAssignedSourcePos && assignedDestination != null)
                    npc?.SetUiStatus("Waiting for task");
                else
                    npc?.SetUiStatus("Idle");
                break;
            case State.MovingToItem:
                {
                    string t = lastTargetType?.ToString() ?? "resource";
                    npc?.SetUiStatus($"Fetching {t}");
                }
                break;
            case State.PickingUp:
                {
                    string t = lastTargetType?.ToString() ?? "resource";
                    npc?.SetUiStatus($"Collecting {t}");
                }
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

    // Nové API: přiřadí úkol sběru od zadané pozice (sourcePos) pro danou building site
    public void AssignCollectionTask(Vector3 sourcePos, IBuildingSite deliverySite)
    {
        if (deliverySite == null) return;

        assignedSourcePos = sourcePos;
        hasAssignedSourcePos = true;
        assignedDestination = deliverySite;

        // Aktualizuj inventory: pokud budova už nepotřebuje některé položky, odstraň je z inventáře
        inventory.RemoveAll(it => !assignedDestination.NeedsResourceForCollectors(it.type));

        // Pokud máme nějaké potřeby a jsme schopni nést další, začneme zadání plnit
        if (BuildingNeedsAny(assignedDestination))
        {
            StartAssignedCollection();
        }
        else
        {
            // pokud nic nepotřebuje, vyčisti a zůstaň v idle
            ClearAssignment();
        }
    }

    // Externí API: zruší přiřazení úkolu
    public void ClearAssignment()
    {
        hasAssignedSourcePos = false;
        assignedDestination = null;
        targetItem = null;
        targetBuilding = null;
        lastTargetType = null;
        state = State.Idle;
        npc.Stop();

        // Pokud hráč dříve přinesl materiál a my máme v inventáři něco co už není potřeba, zahodíme to
        inventory.Clear();
    }

    private void StartAssignedCollection()
    {
        if (!hasAssignedSourcePos || assignedDestination == null) return;

        // Najdi typy, které budova potřebuje, v prioritním pořadí
        var neededTypes = GetNeededTypesOrdered(assignedDestination);
        if (neededTypes == null || neededTypes.Count == 0)
            return;

        // Hledáme první typ, který budova potřebuje AND je dostupný v okolí assignedSourcePos
        ItemType? chosenType = null;
        IGroundItem nearest = null;
        foreach (var t in neededTypes)
        {
            nearest = FindNearestGroundItemOfType(assignedSourcePos, t);
            if (nearest != null)
            {
                chosenType = t;
                break;
            }
        }

        if (nearest != null && chosenType.HasValue)
        {
            targetItem = nearest;
            lastTargetType = TryGetTypeFromGroundItem(nearest);
            npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
            state = State.MovingToItem;
            return;
        }

        // Pokud tady nejsou žádné dostupné položky v assignedSourcePos -> přejít do waiting stavu u audioSource
        npc.MoveTo(assignedSourcePos);
        state = State.WaitingAtSource;
        waitTimer = 0f;
        return;
    }

    private IBuildingSite FindBuildingAndDeliverFallback()
    {
        // Najde nejbližší budovu, která potřebuje něco z našeho inventáře
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
                    if (d < bestDist) { bestDist = d; best = bs; }
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
            if (gi == null) continue;
            if (gi.Type != type) continue;

            float d = Vector3.Distance(origin, ((MonoBehaviour)gi).transform.position);
            if (d < bestDist) { bestDist = d; best = gi; }
        }
        return best;
    }

    private List<ItemType> GetNeededTypesOrdered(IBuildingSite b)
    {
        var list = new List<ItemType>();
        if (b == null) return list;
        // Pořadí preference: wood, stone, ore (upravit podle potřeby)
        if (b.NeedsResourceForCollectors(ItemType.Wood)) list.Add(ItemType.Wood);
        if (b.NeedsResourceForCollectors(ItemType.Stone)) list.Add(ItemType.Stone);
        if (b.NeedsResourceForCollectors(ItemType.Ore)) list.Add(ItemType.Ore);
        return list;
    }

    private ItemType? GetNextNeededTypeForBuilding(IBuildingSite b)
    {
        var ordered = GetNeededTypesOrdered(b);
        return ordered.Count > 0 ? (ItemType?)ordered[0] : null;
    }

    private bool BuildingNeedsAny(IBuildingSite b)
    {
        if (b == null) return false;
        return b.NeedsResourceForCollectors(ItemType.Wood) || b.NeedsResourceForCollectors(ItemType.Stone) || b.NeedsResourceForCollectors(ItemType.Ore);
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
            lastTargetType = TryGetTypeFromGroundItem(best);
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

        // uložíme typ, i když objekt může být ihned zničen
        lastTargetType = gi.Type;
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
                if (bs.NeedsResourceForCollectors(it.type))
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

        // Dodáme všechny položky, které budova potřebuje — po dodání odstraníme z inventáře
        var delivered = new List<(ItemType type, int amount)>();
        foreach (var it in inventory)
        {
            if (targetBuilding.NeedsResourceForCollectors(it.type))
            {
                targetBuilding.AddResourceByCollector(it.type, it.amount);
                delivered.Add(it);
            }
        }

        // Odstraň doručené položky z inventáře
        foreach (var d in delivered)
        {
            inventory.RemoveAll(i => i.type == d.type && i.amount == d.amount);
        }
    }

    private int inventoryTotal()
    {
        int s = 0;
        foreach (var it in inventory) s += it.amount;
        return s;
    }

    // Pomocná metoda: zabezpečené získání ItemType z IGroundItem (volat pouze, když objekt existuje)
    private ItemType? TryGetTypeFromGroundItem(IGroundItem gi)
    {
        if (gi == null) return null;
        try
        {
            return gi.Type;
        }
        catch
        {
            return null;
        }
    }
}