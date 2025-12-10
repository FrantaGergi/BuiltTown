using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BuiltTown.NPC;
using TMPro;

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
        public void SetDistriction(string districtStatus) => owner?.SetRowStatus(npc, districtStatus);
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
    }

    public void RegisterOrUpdateNPC(BaseNPC npc, Role role, string displayName, string status = "")
    {
        if (npc == null) return;

        if (!rows.TryGetValue(npc, out var row))
        {
            row = CreateRowForRole(role);
            rows[npc] = row;
        }

        if (row.icon != null) row.icon.sprite = GetIconForRole(role);
        if (row.nameText != null) row.nameText.text = displayName;
        if (row.statusText != null) row.statusText.text = status;

        BindButtons(npc, row);

        // ensure handle exists and cached
        if (!handles.ContainsKey(npc))
            handles[npc] = new NPCRowHandle(this, npc);
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

        if (row.root != null) Destroy(row.root);
        rows.Remove(npc);

        if (handles.ContainsKey(npc))
            handles.Remove(npc);
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

        Transform parent = role switch
        {
            Role.Miner => minersContainer ?? transform,
            Role.Collector => collectorsContainer ?? transform,
            Role.Builder => buildersContainer ?? transform,
            _ => transform
        };

        var go = Instantiate(prefab, parent, false);
        var row = new NPCRow { root = go };

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
       // TODO if (row.statusText != null) row.actionsContainer.FindChild.text = status ?? string.Empty;
    }
}
