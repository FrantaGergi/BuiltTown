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
    }

    public void UnregisterNPC(BaseNPC npc)
    {
        if (npc == null) return;
        if (!rows.TryGetValue(npc, out var row)) return;

        if (row.root != null) Destroy(row.root);
        rows.Remove(npc);
    }

    public void ClearAll()
    {
        foreach (var kv in rows)
        {
            if (kv.Value?.root != null) Destroy(kv.Value.root);
        }
        rows.Clear();
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
}
