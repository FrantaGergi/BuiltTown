using BuiltTown.NPC;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UINPC : MonoBehaviour
{
    [Serializable]
    public struct ButtonIconMapping
    {
        public Role role;
        public Sprite icon;
    }

    public enum Role
    {
        Miner,
        Collector,
        Builder
    }

    [Header("Row Prefabs (one prefab per role)")]
    [Tooltip("Prefab for miner row (expecteuje 3 action buttons inside child 'Actions')")]
    [SerializeField] private GameObject minerRowPrefab = null;
    [Tooltip("Prefab for collector row (expectuje 1 action button inside child 'Actions')")]
    [SerializeField] private GameObject collectorRowPrefab = null;
    [Tooltip("Prefab for builder row (expectuje 1 action button inside child 'Actions')")]
    [SerializeField] private GameObject builderRowPrefab = null;

    [Header("Empty Row Prefab")]
    [SerializeField] private GameObject emptyRowPrefab = null;

    private readonly Dictionary<Role, GameObject> emptyRows = new Dictionary<Role, GameObject>();

    [Header("Section Containers")]
    [SerializeField] private Transform minersContainer = null;
    [SerializeField] private Transform collectorsContainer = null;
    [SerializeField] private Transform buildersContainer = null;

    [Header("Icons")]
    [SerializeField] private List<ButtonIconMapping> icons = new List<ButtonIconMapping>();

    // Generic action: role-specific meaning is decided by game logic (index convention)
    public event Action<BaseNPC, int> OnActionSelected;
    public event Action<BaseNPC> OnRemoveNPC;

    private readonly Dictionary<BaseNPC, NPCRow> rows = new Dictionary<BaseNPC, NPCRow>();
    private readonly Dictionary<BaseNPC, NPCRowHandle> handles = new Dictionary<BaseNPC, NPCRowHandle>();

    // Public handle class each NPC can hold
    public class NPCRowHandle
    {
        private readonly UINPC owner;
        private readonly BaseNPC npc;

        internal NPCRowHandle(UINPC owner, BaseNPC npc)
        {
            this.owner = owner;
            this.npc = npc;
        }

        public void SetDisplayName(string displayName) => owner?.SetRowName(npc, displayName);
        public void SetStatus(string status) => owner?.SetRowStatus(npc, status);
        public void SetDistrictStatus(string status) => owner?.SetRowDistrictStatus(npc, status);
        public void SetHighlightSection(int index) => owner?.SetHighlightSection(npc, index);
        public void RemoveRow() => owner?.UnregisterNPC(npc);
    }

    private class NPCRow
    {
        public GameObject root;
        public Image icon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI statusText;
        public Transform actionsContainer;
        public Button removeButton;
        public List<Button> actionButtons = new List<Button>();
        public Role role; // uložená role pro tento øádek
    }

    public void RegisterOrUpdateNPC(BaseNPC npc, Role role, string displayName, string status = "")
    {
        if (npc == null) return;

        if (!rows.TryGetValue(npc, out var row))
        {
            row = CreateRowForRole(role);
            rows[npc] = row;
        }

        // uložíme roli
        row.role = role;

        if (row.icon != null) row.icon.sprite = GetIconForRole(role);
        if (row.nameText != null) row.nameText.text = displayName;
        if (row.statusText != null) row.statusText.text = status;

        // Inicializuj další UI elementy (nepøepisovat status)
        SetHighlightSection(npc, -1);
        SetRowDistrictStatus(npc, "");

        BindButtons(npc, row);

        // ensure handle exists and cached
        if (!handles.ContainsKey(npc))
            handles[npc] = new NPCRowHandle(this, npc);

        HideEmptyRow(role);
    }

    // Returns handle; ensures row exists (creates if necessary)
    public NPCRowHandle GetOrCreateHandle(BaseNPC npc, Role role, string displayName = null, string status = null)
    {
        if (npc == null) return null;
        RegisterOrUpdateNPC(npc, role, displayName ?? (npc.name), status ?? string.Empty);
        if (!handles.TryGetValue(npc, out var h))
        {
            h = new NPCRowHandle(this, npc);
            handles[npc] = h;
        }
        return h;
    }

    public void UnregisterNPC(BaseNPC npc)
    {
        if (npc == null) return;
        if (!rows.TryGetValue(npc, out var row)) return;

        // zapamatuj roli pøed odstranìním
        Role role = row.role;

        if (row.root != null) Destroy(row.root);
        rows.Remove(npc);

        if (handles.ContainsKey(npc))
            handles.Remove(npc);

        // ukaž empty row pokud je potøeba pro tuto sekci
        ShowEmptyRowIfNeeded(role);

        // NE nièíme samotný NPC gameObject zde — to by mìlo dìlat spravce NPC pokud je to tøeba
    }

    public void ClearAll()
    {
        foreach (var kv in rows)
        {
            if (kv.Value?.root != null) Destroy(kv.Value.root);
        }
        rows.Clear();
        handles.Clear();
    }

    private NPCRow CreateRowForRole(Role role)
    {
        GameObject prefab = role switch
        {
            Role.Miner => minerRowPrefab,
            Role.Collector => collectorRowPrefab,
            Role.Builder => builderRowPrefab,
            _ => null
        };

        if (prefab == null) throw new InvalidOperationException($"Prefab for role {role} is not assigned.");

        Transform parent = GetContainerForRole(role);

        var go = Instantiate(prefab, parent, false);
        var row = new NPCRow { root = go, role = role };

        // Prefer explicit NPCRowView component (robust, supports nested hierarchy).
        var view = go.GetComponentInChildren<NPCRowView>(true);
        if (view != null)
        {
            row.icon = view.Icon;
            row.nameText = view.NameText;
            row.statusText = view.StatusText;
            row.actionsContainer = view.ActionsContainer;
            row.removeButton = view.RemoveButton;
            return row;
        }

        // Fallback: hledání podle jména (pro zpìtnou kompatibilitu)
        row.icon = go.transform.Find("Icon")?.GetComponent<Image>();
        row.nameText = go.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
        row.statusText = go.transform.Find("Status")?.GetComponent<TextMeshProUGUI>();
        row.actionsContainer = go.transform.Find("Actions");
        row.removeButton = go.transform.Find("Remove")?.GetComponent<Button>();
        Debug.LogError("UINPC: NPCRowView component not found on prefab, falling back to name-based search. This may be less reliable.");
        return row;
    }

    private void BindButtons(BaseNPC npc, NPCRow row)
    {
        // Clear previous listeners
        foreach (var b in row.actionButtons) b.onClick.RemoveAllListeners();
        row.actionButtons.Clear();

        if (row.actionsContainer != null)
        {
            var buttons = row.actionsContainer.GetComponentsInChildren<Button>(true);
            row.actionButtons.AddRange(buttons);

            for (int i = 0; i < row.actionButtons.Count; i++)
            {
                int index = i; // capture
                row.actionButtons[i].onClick.RemoveAllListeners();
                row.actionButtons[i].onClick.AddListener(() => OnActionSelected?.Invoke(npc, index));
            }
        }

        if (row.removeButton != null)
        {
            row.removeButton.onClick.RemoveAllListeners();
            row.removeButton.onClick.AddListener(() => OnRemoveNPC?.Invoke(npc));
        }
    }

    private Sprite GetIconForRole(Role role)
    {
        foreach (var m in icons)
        {
            if (m.role == role && m.icon != null) return m.icon;
        }
        return null;
    }

    // internal helpers used by NPCRowHandle
    private void SetRowName(BaseNPC npc, string displayName)
    {
        if (npc == null || !rows.TryGetValue(npc, out var row)) return;
        if (row.nameText != null) row.nameText.text = displayName ?? string.Empty;
    }

    private void SetRowStatus(BaseNPC npc, string status)
    {
        if (npc == null || !rows.TryGetValue(npc, out var row)) return;
        if (row.statusText != null) row.statusText.text = status ?? string.Empty;
    }
    private void SetRowDistrictStatus(BaseNPC npc, string status)
    {
        if (npc == null || !rows.TryGetValue(npc, out var row)) return;
        if (row.actionsContainer?.GetComponentInChildren<TextMeshProUGUI>() != null) 
            row.actionsContainer.GetComponentInChildren<TextMeshProUGUI>().text = status ?? string.Empty;
    }
    /// <summary>
    /// Sets the currently highlighted section by its index.
    /// </summary>
    /// <param name="index">The zero-based index of the section to highlight.</param>
    private void SetHighlightSection(BaseNPC npc, int index)
    {

        if (npc == null || !rows.TryGetValue(npc, out var row)) return;
        if (row.actionButtons.Count <= 1)  return; // nothing to highlight)

        for (int i = 0; i < row.actionButtons.Count; i++)
        {
            var img = row.actionButtons[i].gameObject.GetComponent<Image>();
            if (img == null) continue;
            Color c = img.color;

            if (i == index)
            {
                c.a = 1f;
                img.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);

            }
            else
            {
                c.a = 0f;
                img.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);

            }
            img.color = c;

        }
    }


    private void ShowEmptyRowIfNeeded(Role role)
    {
        int count = 0;
        foreach (var kv in rows)
        {
            if (kv.Value.role == role) count++;
        }

        if (count == 0 && !emptyRows.ContainsKey(role))
        {
            CreateEmptyRow(role);
        }
    }

    private void HideEmptyRow(Role role)
    {
        if (emptyRows.TryGetValue(role, out GameObject emptyRow))
        {
            if (emptyRow != null) Destroy(emptyRow);
            emptyRows.Remove(role);
        }
    }

    private void CreateEmptyRow(Role role)
    {
        if (emptyRowPrefab == null) return;

        Transform parent = GetContainerForRole(role);
        GameObject emptyRow = Instantiate(emptyRowPrefab, parent, false);

        var textComponent = emptyRow.GetComponentInChildren<TextMeshProUGUI>(true);
        if (textComponent != null)
        {
            textComponent.text = GetEmptyMessageForRole(role);
        }

        emptyRows[role] = emptyRow;
    }

    private string GetEmptyMessageForRole(Role role)
    {
        return role switch
        {
            Role.Miner => "No miners hired\nHire miners to gather resources",
            Role.Collector => "No collectors hired\nHire collectors to transport materials",
            Role.Builder => "No builders hired\nHire builders to construct buildings",
            _ => "No NPCs hired"
        };
    }

    private Transform GetContainerForRole(Role role)
    {
        return role switch
        {
            Role.Miner => minersContainer ?? transform,
            Role.Collector => collectorsContainer ?? transform,
            Role.Builder => buildersContainer ?? transform,
            _ => transform
        };
    }

    private void Start()
    {
        ShowEmptyRowIfNeeded(Role.Miner);
        ShowEmptyRowIfNeeded(Role.Collector);
        ShowEmptyRowIfNeeded(Role.Builder);
    }
}
