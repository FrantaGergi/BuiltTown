
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    public ItemSO itemScriptableObject;

    public int stackCurrent;
    public int stackMax;

    public Image IconImage;



 

    private void Start()
    {
        FitToParent();
        stackMax = itemScriptableObject.stackMax;
        if (stackCurrent == 0)
        {
            stackCurrent = 1;
        }
        SetCurrentStackText(transform.parent.parent.GetComponent<InventorySlot>().TextMeshProUGUI);

        if (IconImage == null) 
        { 
            IconImage.GetComponent<Image>();
            Debug.Log("nastavuješ se nìkdy pøes start??");
        }
        if (itemScriptableObject != null) { 
            IconImage.sprite = itemScriptableObject.icon;
        }
    }

    void Update()
    {
        if (itemScriptableObject != null) 
        IconImage.sprite = itemScriptableObject.icon;
    }

    public void FitToParent()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        RectTransform parentRectTransform = transform.parent.GetComponent<RectTransform>();

        rectTransform.sizeDelta = new Vector2(parentRectTransform.rect.width, parentRectTransform.rect.height);

        rectTransform.localPosition = Vector3.zero;

        rectTransform.localScale = Vector3.one;


    }
    
   

    public void SetCurrentStackText(TextMeshProUGUI txt)
    {
     

        if (stackMax == 1)
        {
            txt.text = "";
        }
        else
        {
            
            if(stackCurrent == 1)
                {
                 txt.text = stackCurrent.ToString();

                }
             else

                {
                txt.text = "x" + stackCurrent.ToString();
                }
        }
    }
}
