using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UINPC;

public class NPCManager : MonoBehaviour
{
    private bool isNPCManagerOpen = false;
    private string previousActionMap = "";

    [Header("References")]
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private GameObject mainNPCManagerContainer;
    [SerializeField] private UINPC UINPC;

    [Header("Runtime services (assign in Inspector)")]
    [Tooltip("MinimapManager: použije se pro interaktivní výbìry (building site / world position)")]
    [SerializeField] private MinimapManager minimapManager;

    // interní mapování NPC -> role (UINPC událost nepøedává roli, proto si ji ukládáme)
    private readonly Dictionary<BaseNPC, UINPC.Role> rolesByNpc = new();

    // Default search radii pro fallback chování (když minimapa není pøiøazena)
    private const float fallbackSearchRadius = 40f;

    public bool ISNPCManagerOpen => isNPCManagerOpen;

    void Start()
    {
        mainNPCManagerContainer.SetActive(false);

        if (UINPC != null)
        {
            UINPC.OnActionSelected += HandleActionSelected;
            UINPC.OnRemoveNPC += HandleRemoveNPC;
        }
    }

    void OnDestroy()
    {
        if (UINPC != null)
        {
            UINPC.OnActionSelected -= HandleActionSelected;
            UINPC.OnRemoveNPC -= HandleRemoveNPC;
        }
    }

    // Toggle UI
    public void ToggleNPCManager(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

        if (GameServices.I.uiBuildingMailboxController.IsSomethingOpen())
        {
            GameServices.I.uiBuildingMailboxController.OnCloseMailbox();
            return;
        }

        else if (minimapManager.IsYesOrNoPanelActive)
        {
            return;
        }
        else if(minimapManager.IsMinimapOpen && isNPCManagerOpen)
        {
            minimapManager.CloseMinimap();
            return;

        }
        else if (minimapManager.IsMinimapOpen && minimapManager.IsChooserActive)
        {
            return;
        }
        else if (minimapManager.IsMinimapOpen && !isNPCManagerOpen)
        {
            minimapManager.CloseMinimap();
        }

        isNPCManagerOpen = !isNPCManagerOpen;
        SetNPCManager();
    }
    public void CloseNPCManager()
    {
        isNPCManagerOpen = false;
        SetNPCManager();
    }

    public void SetNPCManager()
    {
        if (isNPCManagerOpen)
        {
            previousActionMap = playerInput.currentActionMap.name;
            playerInput.SwitchCurrentActionMap("UI");

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            mainNPCManagerContainer.SetActive(true);
            GameServices.I.OnNPCManagerOpen();
        }
        else
        {
            playerInput.SwitchCurrentActionMap(previousActionMap);

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            mainNPCManagerContainer.SetActive(false);
        }
    }

    // Registrace / aktualizace NPC — ukládáme roli, UI se aktualizuje
    public void RegisterNPC(BaseNPC npc, UINPC.Role role, string displayName, string status = "")
    {
        if (npc != null)
            rolesByNpc[npc] = role;

        UINPC.RegisterOrUpdateNPC(npc, role, displayName, status);

        // pøiøaï handle do BaseNPC pro pøímé volání z rolí/NPC
        if (UINPC != null && npc != null)
            npc.UiRow = UINPC.GetOrCreateHandle(npc, role, displayName, status);
    }
    public void UpdateNPCStatus(BaseNPC npc, UINPC.Role role, string displayName, string status = "")
    {
        if (npc != null)
            rolesByNpc[npc] = role;

        UINPC.RegisterOrUpdateNPC(npc, role, displayName, status);

        if (UINPC != null && npc != null)
            npc.UiRow = UINPC.GetOrCreateHandle(npc, role, displayName, status);
    }
    public void UnregisterNPC(BaseNPC npc)
    {
        if (npc != null)
            rolesByNpc.Remove(npc);

        if (UINPC != null)
        {
            UINPC.UnregisterNPC(npc);
        }

        if (npc != null)
            npc.UiRow = null;
    }

    private void HandleRemoveNPC(BaseNPC npc)
    {
        if (npc == null) return;
        UnregisterNPC(npc);
        // další logika odstranìní NPC mùže být pøidána zde
    }

    // UI tlaèítka volají sem: (BaseNPC, index)
    private void HandleActionSelected(BaseNPC npc, int index)
    {
        if (npc == null) return;

        if (!rolesByNpc.TryGetValue(npc, out var role))
        {
            Debug.LogWarning($"NPCManager: role pro NPC {npc.name} není známa. Ignoruji akci.");
            return;
        }

        switch (role)
        {
            case UINPC.Role.Miner:
                HandleMinerAction(npc, index);
                break;
            case UINPC.Role.Collector:
                HandleCollectorAction(npc, index);
                break;
            case UINPC.Role.Builder:
                HandleBuilderAction(npc, index);
                break;
            default:
                Debug.LogWarning($"NPCManager: neoèekávaná role {role} pro {npc.name}");
                break;
        }
    }

    // Miner: index 0 = Wood, 1 = Stone, 2 = Ore
    private void HandleMinerAction(BaseNPC npc, int index)
    {
        ItemType requested = index switch
        {
            1 => ItemType.Stone,
            2 => ItemType.Ore,
            _ => ItemType.Wood
        };

        var miner = npc.GetComponent<MinerRole>();
        if (miner == null)
        {
            Debug.LogWarning($"NPCManager: MinerRole nenalezen na {npc.name}");
            return;
        }

        if (minimapManager != null)
        {
            minimapManager.OpenMinimapToGetSourceCoordinates((Vector3 pos) =>
            {
                miner.AssignMiningTask(pos, requested);
                npc.SetHighlightSection(index);
            }, autoClose: true);
            return;
        }

        // fallback: najdi nejbližší resource node a pøiøaï
        var posFallback = FindNearestResourcePosition(npc.transform.position, requested, fallbackSearchRadius);
        if (posFallback.HasValue)
            miner.AssignMiningTask(posFallback.Value, requested);
        else
            Debug.LogWarning($"NPCManager: nelze najít resource typu {requested} pro {npc.name} (minimap chybí).");
    }

    // Collector: jedno tlaèítko => nejdøíve vyber building (destination), poté source position
    private void HandleCollectorAction(BaseNPC npc, int index)
    {
        var collector = npc.GetComponent<CollectorRole>();
        if (collector == null)
        {
            Debug.LogWarning($"NPCManager: CollectorRole nenalezen na {npc.name}");
            return;
        }

        if (minimapManager != null)
        {
            Debug.Log("NPCManager: otevírám minimapu pro výbìr building site (collector).");

            minimapManager.OpenMinimapToGetBuildingSite((BuildingSite site) =>
            {
                if (site == null)
                {
                    Debug.LogWarning("NPCManager: Minimap vybral null building site.");
                    return;
                }
                
                npc.SetUiDistrictSatus("To District: #" + site.ID);
                // Zde èekáme na hráèùv výbìr, callback se spustí, až klikne
                OpenSourcePositionSelection(collector, site);

            }, autoClose: false);

            return;
        }

        // fallback: najdi nearest building site a nearest ground item pos
        var siteFallback = FindNearestBuildingSite(npc.transform.position, fallbackSearchRadius);
        var posFallback = FindNearestGroundItemPosition(npc.transform.position, fallbackSearchRadius);
        if (siteFallback != null && posFallback.HasValue)
            collector.AssignCollectionTask(posFallback.Value, siteFallback);
        else
            Debug.LogWarning($"NPCManager: nelze najít building site nebo source pozici pro {npc.name} (minimap chybí).");
    }

    // oddìlená metoda pro výbìr source pozice
    private void OpenSourcePositionSelection(CollectorRole collector, BuildingSite site)
    {
        Debug.Log("NPCManager: otevírám minimapu pro výbìr source pozice (collector).");
        // tady je problém že kdyý 
        minimapManager.OpenMinimapToGetSourceCoordinates((Vector3 sourcePos) =>
        {
            collector.AssignCollectionTask(sourcePos, site);
        },true);
    }


    // Builder: jedno tlaèítko => vyber building site kam bude dìlat
    private void HandleBuilderAction(BaseNPC npc, int index)
    {
        var builder = npc.GetComponent<BuilderRole>();
        if (builder == null)
        {
            Debug.LogWarning($"NPCManager: BuilderRole nenalezen na {npc.name}");
            return;
        }

        if (minimapManager != null)
        {
            minimapManager.OpenMinimapToGetBuildingSite((BuildingSite site) =>
            {
                if (site == null) 
                { Debug.LogWarning("NPCManager: Minimap vybral null building site."); return; }
                npc.SetUiDistrictSatus("At District #" + site.ID);
                builder.AssignBuildingSite(site);
            }, autoClose: true);
            return;
        }

        // fallback: najdi nearest building site a pøiøaï
        var siteFallback = FindNearestBuildingSite(npc.transform.position, fallbackSearchRadius);
        if (siteFallback != null)
            builder.AssignBuildingSite(siteFallback);
        else
            Debug.LogWarning($"NPCManager: nelze najít building site pro {npc.name} (minimap chybí).");
    }

    // --- Fallback utility metody (pouze když minimapa není pøiøazena) ---

    private Vector3? FindNearestResourcePosition(Vector3 origin, ItemType type, float radius)
    {
        Collider[] cols = Physics.OverlapSphere(origin, radius);
        Vector3? bestPos = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var rs = c.GetComponent<IResourceSource>();
            if (rs == null) continue;
            if (rs.Type != type) continue;
            if (!rs.CanMine()) continue;
            var pos = ((Component)rs).transform.position;
            float d = Vector3.Distance(origin, pos);
            if (d < bestDist) { bestDist = d; bestPos = pos; }
        }
        return bestPos;
    }

    private Vector3? FindNearestGroundItemPosition(Vector3 origin, float radius)
    {
        Collider[] cols = Physics.OverlapSphere(origin, radius);
        Vector3? bestPos = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var gi = c.GetComponent<IGroundItem>();
            if (gi == null) continue;
            var pos = ((Component)gi).transform.position;
            float d = Vector3.Distance(origin, pos);
            if (d < bestDist) { bestDist = d; bestPos = pos; }
        }
        return bestPos;
    }

    private IBuildingSite FindNearestBuildingSite(Vector3 origin, float radius)
    {
        Collider[] cols = Physics.OverlapSphere(origin, radius);
        IBuildingSite best = null;
        float bestDist = float.MaxValue;
        foreach (var c in cols)
        {
            var bs = c.GetComponent<IBuildingSite>();
            if (bs == null) continue;
            var pos = ((Component)bs).transform.position;
            float d = Vector3.Distance(origin, pos);
            if (d < bestDist) { bestDist = d; best = bs; }
        }
        return best;
    }
}
