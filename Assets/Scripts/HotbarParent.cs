using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class HotbarParent : InventoryBase
{
   
    [Header("this let like it is")]
    [SerializeField]
    public int selectedHotbarSlot = 0; // if not -1 is selected, if is -1, nothing is active
    [SerializeField]
    public int lastSelectedhotbarSlot = 0;

    protected override void Awake() // the first thing what we want,bcs in inv_Manager we are calling the method wich is op
    {
       base.Awake();
        ClearTextInSlots();
    }
    private void Start()
    {
    }

    public bool IsHotbarChange()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            lastSelectedhotbarSlot = selectedHotbarSlot;

            selectedHotbarSlot = 0;
            if (lastSelectedhotbarSlot == selectedHotbarSlot)
            {
                selectedHotbarSlot = -1;
            }


            return true;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            lastSelectedhotbarSlot = selectedHotbarSlot;

            selectedHotbarSlot = 1;
            if (lastSelectedhotbarSlot == selectedHotbarSlot)
            {
                selectedHotbarSlot = -1;
            }

            return true;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            lastSelectedhotbarSlot = selectedHotbarSlot;

            selectedHotbarSlot = 2;
            if (lastSelectedhotbarSlot == selectedHotbarSlot)
            {
                selectedHotbarSlot = -1;
            }

            return true;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            lastSelectedhotbarSlot = selectedHotbarSlot;

            selectedHotbarSlot = 3;
            if (lastSelectedhotbarSlot == selectedHotbarSlot)
            {
                selectedHotbarSlot = -1;
            }

            return true;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            lastSelectedhotbarSlot = selectedHotbarSlot;

            selectedHotbarSlot = 4;
            if (lastSelectedhotbarSlot == selectedHotbarSlot)
            {
                selectedHotbarSlot = -1;
            }

            return true;

        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            lastSelectedhotbarSlot = selectedHotbarSlot;

            selectedHotbarSlot = 5;
            if (lastSelectedhotbarSlot == selectedHotbarSlot)
            {
                selectedHotbarSlot = -1;
            }

            return true;

        }
        return false;
    }

  

 

  
}
