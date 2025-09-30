using TMPro;
using UnityEngine;

public class HotbarParent : MonoBehaviour
{
    public Color woodColor = new Color(196, 124, 61, 200); // SaddleBrown
    public Color stoneColor = new Color(168, 176, 184,200); // Gray
    public Color oreColor = new Color(74, 91, 169,200);   // DarkBlue
    public Color emptyColor = new Color(30,30,30, 160); // light black

    public HotbarSlot[] slots = new HotbarSlot[3];


    private void Start()
    {
        foreach (var slot in slots)
        {
            slot.Start(GetColorFromSO(null));
        }
    }

    public void SetSlot(int index, ItemSO item, int count)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index].SetItem(item, count,GetColorFromSO(item));
    }

    public void ClearSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;
        slots[index].Clear(emptyColor);
    }

    private Color GetColorFromSO(ItemSO itemSO)
    {
        if (itemSO == null) return emptyColor;

        switch (itemSO.ItemName.ToLower())
        {
            case "wood":
                 return woodColor;
            case "stone":
                 return stoneColor;
            case "ore":
                 return oreColor;
            default:
                 return emptyColor;
        }
    }
}

[System.Serializable]
public class HotbarSlot
{
    public ItemSO item;
    public int count;

    [Header("UI References")]
    [SerializeField]
    private UnityEngine.UI.Image backgroundImage;
    [SerializeField]
    private UnityEngine.UI.Image itemImage;
    [SerializeField]
    private TextMeshProUGUI countText;

    public void Start(Color emptyColour)
    {
        Update(emptyColour);
    }

    public void SetItem(ItemSO newItem, int newCount, Color bckColor)
    {
        item = newItem;
        count = newCount;
        Update(bckColor);

    }

    public void Clear(Color bckColor)
    {
        item = null;
        count = 0;
        Update(bckColor);
    }


    private void Update(Color bckColor)
    {
        if (countText != null)
            countText.text = count >= 1 ? count.ToString() : "";

        if (itemImage != null) 
        { 
            if (item != null && item.icon != null)
            {
                itemImage.sprite = item.icon;
                itemImage.enabled = true;
            }
            else
            {
                itemImage.sprite = null;
                itemImage.enabled = false;
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = bckColor;
        }
    }
}
