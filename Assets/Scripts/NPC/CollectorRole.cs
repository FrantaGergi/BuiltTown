using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CollectorRole : NPCRoleBase
{
    [Header("Behaviour")]
    [SerializeField] private bool autoSearch = false;
    public float searchRadius = 20f;
    public int capacity = 5;

    private List<(ItemType type, int amount)> inventory = new();
    private IGroundItem targetItem;
    private IBuildingSite targetBuilding;

    // ExternÌ p¯i¯azenÌ ˙kolu: sou¯adnice odkud hledat + kam doruËit
    private Vector3 assignedSourcePos;
    private bool hasAssignedSourcePos = false;
    private IBuildingSite assignedDestination;

    private enum State { Idle, MovingToItem, PickingUp, MovingToBuilding, Depositing }
    private State state = State.Idle;

    void Update()
    {
        // Pokud je budova dokonËena, vyËistit assignment a jÌt do idle
        if (assignedDestination != null && !BuildingNeedsAny(assignedDestination))
        {
            ClearAssignment(); // vËetnÏ vymaz·nÌ p¯idÏlenÈ sourcePos
        }

        switch (state)
        {
            case State.Idle:
                // Pokud m·me explicitnÌ ˙kol, spusù ho
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
                // Po pickupu rozhodneme, kam jÌt d·l v z·vislosti na pot¯ebÏ budovy
                // 1) Kapacita pln· -> okamûitÏ doruË
                if (inventoryTotal() >= capacity)
                {
                    if (assignedDestination != null)
                        targetBuilding = assignedDestination;
                    else
                        targetBuilding = FindBuildingAndDeliverFallback();

                    if (targetBuilding != null)
                    {
                        npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                        state = State.MovingToBuilding;
                        return;
                    }
                }

                // 2) M·me ˙kol -> budova st·le pot¯ebuje -> pokraËuj v hled·nÌ dalöÌho itemu
                if (assignedDestination != null && BuildingNeedsAny(assignedDestination))
                {
                    // pokud budova st·le pot¯ebuje, hledej dalöÌ item podle assignedSourcePos
                    targetItem = null;
                    StartAssignedCollection(); // on najde dalöÌ item spr·vnÈho typu (nebo poöle doruËenÌ)
                    return;
                }

                // 3) AutoSearch verze
                targetItem = null;
                state = State.Idle;
                break;
            case State.MovingToBuilding:
                if (targetBuilding == null) { state = State.Idle; break; }
                if (npc.IsAtDestination()) { state = State.Depositing; npc.Stop(); }
                break;
            case State.Depositing:
                if (targetBuilding == null) { state = State.Idle; break; }
                DepositAll();
                // pokud to byl explicitnÌ ˙kol, po dod·nÌ znovu zkontrolujeme dalöÌ pot¯eby
                if (hasAssignedSourcePos && assignedDestination != null && BuildingNeedsAny(assignedDestination))
                {
                    // zaËneme znovu hledat dalöÌ resource od assignedSourcePos
                    StartAssignedCollection();
                }
                else
                {
                    // pokud ˙kol dokonËen nebo û·dn· dalöÌ pot¯eba, vyËistit assignment
                    if (hasAssignedSourcePos)
                        ClearAssignment();
                }
                state = State.Idle;
                break;
        }
    }

    // NovÈ API: p¯i¯adÌ ˙kol sbÏru od zadanÈ pozice (sourcePos) pro danou building site
    public void AssignCollectionTask(Vector3 sourcePos, IBuildingSite deliverySite)
    {
        if (deliverySite == null) return;

        assignedSourcePos = sourcePos;
        hasAssignedSourcePos = true;
        assignedDestination = deliverySite;

        // Aktualizuj inventory: pokud budova uû nepot¯ebuje nÏkterÈ poloûky, odstraÚ je z invent·¯e
        inventory.RemoveAll(it => !assignedDestination.NeedsResource(it.type));

        // Pokud m·me nÏjakÈ pot¯eby a jsme schopni nÈst dalöÌ, zaËneme zad·nÌ plnit
        if (BuildingNeedsAny(assignedDestination))
        {
            StartAssignedCollection();
        }
        else
        {
            // pokud nic nepot¯ebuje, vyËisti a z˘staÚ v idle
            ClearAssignment();
        }
    }

    // ExternÌ API: zruöÌ p¯i¯azenÌ ˙kolu
    public void ClearAssignment()
    {
        hasAssignedSourcePos = false;
        assignedDestination = null;
        targetItem = null;
        targetBuilding = null;
        state = State.Idle;
        npc.Stop();

        // Pokud hr·Ë d¯Ìve p¯inesl materi·l a my m·me v invent·¯i nÏco co uû nenÌ pot¯eba, zahodÌme to
        inventory.Clear();
    }

    private void StartAssignedCollection()
    {
        if (!hasAssignedSourcePos || assignedDestination == null) return;

        // Najdi typy, kterÈ budova pot¯ebuje, v prioritnÌm po¯adÌ
        var neededTypes = GetNeededTypesOrdered(assignedDestination);
        if (neededTypes == null || neededTypes.Count == 0)
            return;

        // Hled·me prvnÌ typ, kter˝ budova pot¯ebuje AND je dostupn˝ v okolÌ assignedSourcePos
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
            npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
            state = State.MovingToItem;
            return;
        }

        // Pokud tady nejsou û·dnÈ dostupnÈ poloûky v assignedSourcePos:
        // 1) pokud uû m·me v invent·¯i nÏco, zkuste to doruËit do budovy (pokud budova st·le pot¯ebuje)
        if (inventory.Count > 0)
        {
            // najdeme prvnÌ typ v invent·¯i, kter˝ budova p¯ijme
            var invAccepted = inventory.FirstOrDefault(it => assignedDestination.NeedsResource(it.type));
            if (!invAccepted.Equals(default((ItemType, int))))
            {
                targetBuilding = assignedDestination;
                npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                state = State.MovingToBuilding;
                return;
            }

            // pokud nic v invent·¯i nevyhovuje pot¯eb·m, zahodÌme invent·¯ a zkusÌme najÌt jin˝ typ co budova jeötÏ pot¯ebuje a je k nalezenÌ v okolÌ
            inventory.Clear();

            // zkus najÌt jak˝koli pot¯ebn˝ typ v okolÌ assignedSourcePos (bez ohledu na po¯adÌ)
            foreach (var t in neededTypes)
            {
                var f = FindNearestGroundItemOfType(assignedSourcePos, t);
                if (f != null)
                {
                    targetItem = f;
                    npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
                    state = State.MovingToItem;
                    return;
                }
            }
        }

        // 2) fallback: zkus najÌt jak˝koli pot¯ebn˝ typ v okolÌ NPC
        foreach (var t in neededTypes)
        {
            var f = FindNearestGroundItemOfType(((MonoBehaviour)npc).transform.position, t);
            if (f != null)
            {
                targetItem = f;
                npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
                state = State.MovingToItem;
                return;
            }
        }

        // 3) nic nenalezeno -> pokud budova st·le nÏco pot¯ebuje, doruË to co m·me (i kdyû pr·zdnÈ -> nic nedÏlej)
        if (inventory.Count > 0 && BuildingNeedsAny(assignedDestination))
        {
            targetBuilding = assignedDestination;
            npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
            state = State.MovingToBuilding;
            return;
        }

        // 4) pokud nic k dispozici a nic v invent·¯i, ukonËi assignment
        Debug.Log($"CollectorRole: nelze najÌt û·dnÈ poloûky pro assignedSourcePos {assignedSourcePos} a budova {assignedDestination}. UkonËuji ˙kol.");
        ClearAssignment();
    }

    private IBuildingSite FindBuildingAndDeliverFallback()
    {
        // Najde nejbliûöÌ budovu, kter· pot¯ebuje nÏco z naöeho invent·¯e
        Collider[] cols = Physics.OverlapSphere(((MonoBehaviour)npc).transform.position, searchRadius);
        IBuildingSite best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var bs = c.GetComponent<IBuildingSite>();
            if (bs == null) continue;
            foreach (var it in inventory)
            {
                if (bs.NeedsResource(it.type))
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
        // Po¯adÌ preference: wood, stone, ore (upravit podle pot¯eby)
        if (b.NeedsResource(ItemType.Wood)) list.Add(ItemType.Wood);
        if (b.NeedsResource(ItemType.Stone)) list.Add(ItemType.Stone);
        if (b.NeedsResource(ItemType.Ore)) list.Add(ItemType.Ore);
        return list;
    }



    private bool BuildingNeedsAny(IBuildingSite b)
    {
        if (b == null) return false;
        return b.NeedsResource(ItemType.Wood) || b.NeedsResource(ItemType.Stone) || b.NeedsResource(ItemType.Ore);
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

        // Dod·me vöechny poloûky, kterÈ budova pot¯ebuje ó po dod·nÌ odstranÌme z invent·¯e
        var delivered = new List<(ItemType type, int amount)>();
        foreach (var it in inventory)
        {
            if (targetBuilding.NeedsResource(it.type))
            {
                targetBuilding.AddResource(it.type, it.amount);
                delivered.Add(it);
            }
        }

        // OdstraÚ doruËenÈ poloûky z invent·¯e
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
}
