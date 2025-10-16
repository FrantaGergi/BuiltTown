using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
    public string ItemName;
    public Sprite icon;
    public GameObject prefab;
    public Material HighlightedMaterial;
    public ItemType itemType;
    public string description;
    public int price;
}

public enum ItemType
{
    None,
    Chopp,
    Mine,
    Wood,
    Stone,
    Ore,
}