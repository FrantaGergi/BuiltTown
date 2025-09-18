using TMPro;
using UnityEngine;

public class HotbarParent : MonoBehaviour
{
    public HotbarSlot[] slots = new HotbarSlot[3];

    public void SetSlot(int index, ItemSO item, int count)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index].SetItem(item, count);
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index].Clear();
    }
}

[System.Serializable]
public class HotbarSlot
{
    public ItemSO item;
    public int count;

    [Header("UI References")]
    [SerializeField]
    private UnityEngine.UI.Image itemImage;
    [SerializeField]
    private TextMeshProUGUI countText;


    public void SetItem(ItemSO newItem, int newCount)
    {
        item = newItem;
        count = newCount;
        Update();

    }

    public void Clear()
    {
        item = null;
        count = 0;
        Update();
    }

    private void Update()
    {
        if (countText != null)
            countText.text = count > 1 ? count.ToString() : "";

        if (itemImage != null)
            itemImage.sprite = item != null ? item.icon : null;
    }
}
