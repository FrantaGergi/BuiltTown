using UnityEngine;
using System.Collections.Generic;

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
                if (assignedDestination != null && assignedDestination.NeedsResource(((GroundItem)targetItem).Type))
                {
                    // doruËi na budovu
                    targetBuilding = assignedDestination;
                    npc.MoveTo(((MonoBehaviour)targetBuilding).transform.position);
                    state = State.MovingToBuilding;
                }
                else
                {
                    // nechat hledat dalöÌ (nebo autoSearch)
                    targetItem = null;
                    state = State.Idle;
                }
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

        // Najdi typ, kter˝ budova pot¯ebuje a kter˝ zatÌm nem·me v invent·¯i (nebo m˘ûeme mÌt vÌce)
        ItemType? neededType = GetNextNeededTypeForBuilding(assignedDestination);
        if (neededType == null)
        {
            // nic nepot¯ebuje
            return;
        }

        // Najdi nejbliûöÌ GroundItem danÈho typu z assignedSourcePos
        IGroundItem nearest = FindNearestGroundItemOfType(assignedSourcePos, neededType.Value);
        if (nearest != null)
        {
            targetItem = nearest;
            npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
            state = State.MovingToItem;
        }
        else
        {
            // nic v dosahu na assignedSourcePos; m˘ûeme buÔ rozöÌ¯it hled·nÌ, nebo Ëekat/ukonËit
            Debug.Log($"CollectorRole: û·dn˝ GroundItem typu {neededType} nalezen v okolÌ assignedSourcePos {assignedSourcePos}.");
            // Zkus najÌt glob·lnÏ kolem NPC jako fallback
            IGroundItem fallback = FindNearestGroundItemOfType(((MonoBehaviour)npc).transform.position, neededType.Value);
            if (fallback != null)
            {
                targetItem = fallback;
                npc.MoveTo(((MonoBehaviour)targetItem).transform.position);
                state = State.MovingToItem;
            }
            else
            {
                // nelze najÌt -> poËkej nebo zruö ˙kol
                // nech·me b˝t v idle a Ëek·me na dalöÌ moûnosti
            }
        }
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

    private ItemType? GetNextNeededTypeForBuilding(IBuildingSite b)
    {
        // Kontroluj z·kladnÌ typy (dle existence v projektu). Pokud m·ö vÌc typ˘, p¯idej je sem.
        if (b.NeedsResource(ItemType.Wood)) return ItemType.Wood;
        if (b.NeedsResource(ItemType.Stone)) return ItemType.Stone;
        if (b.NeedsResource(ItemType.Ore)) return ItemType.Ore;
        return null;
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
