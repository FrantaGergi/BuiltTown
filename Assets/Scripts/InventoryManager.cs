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
            if (slot.item == itemSO)
                return slot.count;
        }
        return 0;
    }

 

    // Zde mùžete ponechat metody pro UI, drag&drop atd. podle potøeby
    public void OnPointerDown(PointerEventData eventData) { /* ... */ }
    public void OnPointerUp(PointerEventData eventData) { /* ... */ }
}