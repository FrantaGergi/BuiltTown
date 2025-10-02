using System.Collections;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Item", menuName = "Scriptable Objects/Item")]
public class ItemSO : ScriptableObject
{
    public string ItemName;
    public Sprite icon;
    public Material HighlightedMaterial;
    public ItemType itemType;
    public string description;
    public int price;
}

public enum ItemType
{
    None,
    Chopp,
    Mine
}