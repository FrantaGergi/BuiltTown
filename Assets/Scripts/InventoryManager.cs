using UnityEngine;
using UnityEngine.EventSystems;

public class InventoryManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] HotbarParent hotbarParent;

    // Pøidání suroviny do hotbaru (pøi tìžení)
    public void AddResourceToHotbar(ItemSO itemSO, int amount)
    {
        for (int i = 0; i < hotbarParent.slots.Length; i++)
        {
            var slot = hotbarParent.slots[i];
            if (slot.item == itemSO)
            {
                slot.count += amount;
                hotbarParent.SetSlot(i, itemSO, slot.count);
                return;
            }
        }
        // Najdi prázdný slot
        for (int i = 0; i < hotbarParent.slots.Length; i++)
        {
            var slot = hotbarParent.slots[i];
            if (slot.item == null)
            {
                hotbarParent.SetSlot(i, itemSO, amount);
                return;
            }
        }
        // Hotbar plný – surovinu nelze pøidat
    }

    // Odebrání suroviny z hotbaru (pøi doruèení)
    public bool RemoveResourceFromHotbar(ItemSO itemSO, int amount)
    {
        for (int i = 0; i < hotbarParent.slots.Length; i++)
        {
            var slot = hotbarParent.slots[i];
            if (slot.item == itemSO && slot.count >= amount)
            {
                slot.count -= amount;
                if (slot.count <= 0)
                    hotbarParent.ClearSlot(i);
                else
                    hotbarParent.SetSlot(i, itemSO, slot.count);
                return true;
            }
        }
        return false;
    }
  

    // Získání poètu suroviny v hotbaru
    public int GetResourceCount(ItemSO itemSO)
    {
        foreach (var slot in hotbarParent.slots)
        {
            if (slot != null && slot.item != null
                && slot.item == itemSO)
                return slot.count;
        }
        return 0;
    }
    public int GetResourceCount(ItemType itemType)
    {
        foreach (var slot in hotbarParent.slots)
        {
            if (slot != null && slot.item  != null 
                && slot.item.itemType == itemType)
                return slot.count;
        }
        return 0;
    }
    //získání všem item Typù co se nachází v hotbaru
    public ItemType[] GetAllItemTypesInHotbar()
    {
        System.Collections.Generic.List<ItemType> itemTypes = new System.Collections.Generic.List<ItemType>();
        foreach (var slot in hotbarParent.slots)
        {
            if (slot != null && slot.item != null
                && !itemTypes.Contains(slot.item.itemType))
            {
                itemTypes.Add(slot.item.itemType);
            }
        }
        return itemTypes.ToArray();
    }
  

    public ItemSO GetItemSOByItemType(ItemType itemType)
    {
        foreach (var slot in hotbarParent.slots)
        {
            if (slot != null && slot.item != null
                && slot.item != null && slot.item.itemType == itemType)
                return slot.item;
        }
        return null;
    }



    // Zde mùžete ponechat metody pro UI, drag&drop atd. podle potøeby
    public void OnPointerDown(PointerEventData eventData) { /* ... */ }
    public void OnPointerUp(PointerEventData eventData) { /* ... */ }
}