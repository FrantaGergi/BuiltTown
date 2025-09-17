using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class InventorySlot : MonoBehaviour
{

   


    [Header("SET ONLY IF YOU HAVE NOW ADDED ITEM IN HOLDERITEM")]
    [SerializeField]
    private InventoryItem heldItem;
    public InventoryItem HeldItem => heldItem;

    [Header("SET")]
    [SerializeField] public TextMeshProUGUI TextMeshProUGUI;
    [SerializeField] public Image Background;
    [SerializeField] public GameObject ItemHolder;




    private void Start()
    {
       heldItem = ItemHolder?.GetComponentInChildren<InventoryItem>();

      
    }

    public void SetHeldItem(InventoryItem item)
    {
        heldItem = item;
        if(heldItem != null) {
            
            item.transform.SetParent(ItemHolder.transform, false);    // set in hiearchy

            item.FitToParent();   // set rectTransform
            item.SetCurrentStackText(TextMeshProUGUI);
        }
        else
        {
            ClearText();

        }
    }

    public void ClearText()
    {
        if(heldItem == null)
        TextMeshProUGUI.text = "";

    }

    public void DeleteItem()
    {
        Destroy(heldItem.gameObject);
        heldItem = null; ////// biggest problém => misiing object not equal to emptyobject
    }
}
