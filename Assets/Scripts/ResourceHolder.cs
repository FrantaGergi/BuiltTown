using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Reprezentuje fyzické úložištì surovin na zemi.
/// Mùže být použito collectory pro ukládání nebo distribuce resources.
/// </summary>
public class ResourceHolder : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] private int maxCapacityPerType = 100;

    [Header("Current Resources")]
    [SerializeField] private int woodCount = 0;
    [SerializeField] private int stoneCount = 0;
    [SerializeField] private int oreCount = 0;

    [Header("UI Reference")]
    [SerializeField] private ResourceHolderUI holderUI;

    // Events pro sledování zmìn
    public event Action<ItemType, int> OnResourceChanged;
    public event Action OnAnyResourceChanged;
    public  ResourceHolderUI ResourceHolderUI => holderUI;

    private Dictionary<ItemType, int> resources = new Dictionary<ItemType, int>();

    void Start()
    {
        InitializeResources();

        if (holderUI == null)
        {
            holderUI = GetComponentInChildren<ResourceHolderUI>();
            if (holderUI == null)
            {
                Debug.LogWarning($"ResourceHolder: UI component not found on {name}");
            }
        }

        UpdateUI();
    }

    private void InitializeResources()
    {
        resources[ItemType.Wood] = woodCount;
        resources[ItemType.Stone] = stoneCount;
        resources[ItemType.Ore] = oreCount;
    }

    #region Add Resources

    /// <summary>
    /// Pøidá urèité množství zdroje. Respektuje maximální kapacitu.
    /// </summary>
    /// <returns>Množství skuteènì pøidaného zdroje</returns>
    public int AddResource(ItemType type, int amount)
    {
        if (amount <= 0) return 0;
        if (!IsValidResourceType(type)) return 0;

        int current = GetResourceCount(type);
        int available = maxCapacityPerType - current;
        int toAdd = Mathf.Min(amount, available);

        if (toAdd > 0)
        {
            resources[type] = current + toAdd;
            SyncSerializedFields();
            OnResourceChanged?.Invoke(type, resources[type]);
            OnAnyResourceChanged?.Invoke();
            UpdateUI();
        }

        return toAdd;
    }

    /// <summary>
    /// Pokusí se pøidat všechny zdroje. Vrací množství, které se nepodaøilo pøidat.
    /// </summary>
    public Dictionary<ItemType, int> AddResourcesBatch(Dictionary<ItemType, int> resourcesToAdd)
    {
        var overflow = new Dictionary<ItemType, int>();

        foreach (var kvp in resourcesToAdd)
        {
            int added = AddResource(kvp.Key, kvp.Value);
            int remaining = kvp.Value - added;

            if (remaining > 0)
                overflow[kvp.Key] = remaining;
        }

        return overflow;
    }

    #endregion

    #region Remove Resources

    /// <summary>
    /// Odebere urèité množství zdroje.
    /// </summary>
    /// <returns>Množství skuteènì odebraného zdroje</returns>
    public int RemoveResource(ItemType type, int amount)
    {
        if (amount <= 0) return 0;
        if (!IsValidResourceType(type)) return 0;

        int current = GetResourceCount(type);
        int toRemove = Mathf.Min(amount, current);

        if (toRemove > 0)
        {
            resources[type] = current - toRemove;
            SyncSerializedFields();
            OnResourceChanged?.Invoke(type, resources[type]);
            OnAnyResourceChanged?.Invoke();
            UpdateUI();
        }

        return toRemove;
    }

    /// <summary>
    /// Odebere všechny zdroje daného typu.
    /// </summary>
    /// <returns>Množství odebraného zdroje</returns>
    public int RemoveAllOfType(ItemType type)
    {
        int current = GetResourceCount(type);
        return RemoveResource(type, current);
    }

    /// <summary>
    /// Odebere všechny zdroje všech typù.
    /// </summary>
    /// <returns>Dictionary obsahující množství každého typu</returns>
    public Dictionary<ItemType, int> RemoveAllResources()
    {
        var removed = new Dictionary<ItemType, int>();

        foreach (var type in GetAllResourceTypes())
        {
            int amount = RemoveAllOfType(type);
            if (amount > 0)
                removed[type] = amount;
        }

        return removed;
    }

    /// <summary>
    /// Pokusí se odebrat požadované množství. Vrací skuteènì odebrané množství.
    /// </summary>
    public Dictionary<ItemType, int> RemoveResourcesBatch(Dictionary<ItemType, int> resourcesToRemove)
    {
        var removed = new Dictionary<ItemType, int>();

        foreach (var kvp in resourcesToRemove)
        {
            int amount = RemoveResource(kvp.Key, kvp.Value);
            if (amount > 0)
                removed[kvp.Key] = amount;
        }

        return removed;
    }

    #endregion

    #region Transfer

    /// <summary>
    /// Pøesune zdroje z tohoto holderu do jiného.
    /// </summary>
    /// <returns>Dictionary s množstvím skuteènì pøesunutých zdrojù</returns>
    public Dictionary<ItemType, int> TransferTo(ResourceHolder target, Dictionary<ItemType, int> resourcesToTransfer)
    {
        if (target == null) return new Dictionary<ItemType, int>();

        var transferred = new Dictionary<ItemType, int>();

        foreach (var kvp in resourcesToTransfer)
        {
            int removed = RemoveResource(kvp.Key, kvp.Value);
            if (removed > 0)
            {
                int added = target.AddResource(kvp.Key, removed);
                transferred[kvp.Key] = added;

                // Pokud se nepodaøilo pøidat vše, vra zbytek zpìt
                int overflow = removed - added;
                if (overflow > 0)
                    AddResource(kvp.Key, overflow);
            }
        }

        return transferred;
    }

    /// <summary>
    /// Pøesune všechny zdroje do jiného holderu.
    /// </summary>
    public Dictionary<ItemType, int> TransferAllTo(ResourceHolder target)
    {
        var allResources = GetAllResourceCounts();
        return TransferTo(target, allResources);
    }

    #endregion

    #region Query Methods

    /// <summary>
    /// Vrací poèet zdroje daného typu.
    /// </summary>
    public int GetResourceCount(ItemType type)
    {
        if (!IsValidResourceType(type)) return 0;
        return resources.ContainsKey(type) ? resources[type] : 0;
    }

    /// <summary>
    /// Vrací dictionary se všemi zdroji.
    /// </summary>
    public Dictionary<ItemType, int> GetAllResourceCounts()
    {
        return new Dictionary<ItemType, int>(resources);
    }

    /// <summary>
    /// Kontroluje, zda holder obsahuje alespoò požadované množství.
    /// </summary>
    public bool HasResource(ItemType type, int amount)
    {
        return GetResourceCount(type) >= amount;
    }

    /// <summary>
    /// Kontroluje, zda holder obsahuje všechny požadované zdroje.
    /// </summary>
    public bool HasResources(Dictionary<ItemType, int> required)
    {
        foreach (var kvp in required)
        {
            if (!HasResource(kvp.Key, kvp.Value))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Vrací dostupnou kapacitu pro daný typ.
    /// </summary>
    public int GetAvailableCapacity(ItemType type)
    {
        return maxCapacityPerType - GetResourceCount(type);
    }

    /// <summary>
    /// Kontroluje, zda je holder prázdný.
    /// </summary>
    public bool IsEmpty()
    {
        foreach (var type in GetAllResourceTypes())
        {
            if (GetResourceCount(type) > 0)
                return false;
        }
        return true;
    }

    /// <summary>
    /// Kontroluje, zda je holder plný pro daný typ.
    /// </summary>
    public bool IsFull(ItemType type)
    {
        return GetResourceCount(type) >= maxCapacityPerType;
    }

    public ItemType GetItemInHolder()
    {
        foreach (var type in GetAllResourceTypes())
        {
            if (GetResourceCount(type) > 0)
                return type;
        }
        return ItemType.None;
    }

    #endregion

    #region Helper Methods

    private bool IsValidResourceType(ItemType type)
    {
        return type == ItemType.Wood || type == ItemType.Stone || type == ItemType.Ore;
    }

    private ItemType[] GetAllResourceTypes()
    {
        return new[] { ItemType.Wood, ItemType.Stone, ItemType.Ore };
    }


    private void SyncSerializedFields()
    {
        woodCount = resources[ItemType.Wood];
        stoneCount = resources[ItemType.Stone];
        oreCount = resources[ItemType.Ore];
    }

    private void UpdateUI()
    {
        if (holderUI != null)
        {
            holderUI.UpdateDisplay(resources[ItemType.Wood], resources[ItemType.Stone], resources[ItemType.Ore]);
        }
    }

    #endregion

    #region Debug Methods

    [ContextMenu("Debug: Add 10 Wood")]
    private void DebugAdd10Wood() => AddResource(ItemType.Wood, 10);

    [ContextMenu("Debug: Add 10 Stone")]
    private void DebugAdd10Stone() => AddResource(ItemType.Stone, 10);

    [ContextMenu("Debug: Add 10 Ore")]
    private void DebugAdd10Ore() => AddResource(ItemType.Ore, 10);

    [ContextMenu("Debug: Remove All")]
    private void DebugRemoveAll() => RemoveAllResources();

    [ContextMenu("Debug: Fill Capacity")]
    private void DebugFillCapacity()
    {
        AddResource(ItemType.Wood, maxCapacityPerType);
        AddResource(ItemType.Stone, maxCapacityPerType);
        AddResource(ItemType.Ore, maxCapacityPerType);
    }

    #endregion
}