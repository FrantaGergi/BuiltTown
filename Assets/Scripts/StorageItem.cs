using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class StorageItem
{
    public int currentStack;
    public ItemSO itemScriptableObject;

    public StorageItem(int currentStack, ItemSO itemScriptableObject)
    {
        this.currentStack = currentStack;
        this.itemScriptableObject = itemScriptableObject;
    }
}
