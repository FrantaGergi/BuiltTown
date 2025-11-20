using Synty.AnimationBaseLocomotion.Samples;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private IKHandler ikHandler;
    [SerializeField] private ToolIconTransition uiTransition;
    [SerializeField] private Image PrimaryImage;
    [SerializeField] private Image SecondaryImage;
    [Header("Tools")]
    [SerializeField] private ItemSO primary;
    public ItemSO Primary
    {
        get => primary;
        set
        {
            primary = value;
            PrimaryImage.sprite = primary.icon;
        }
    }
    [SerializeField] private ItemSO Secundary;
    public ItemSO Secondary
    {
        get => Secundary;
        set
        {
            Secundary = value;
            SecondaryImage.sprite = Secundary.icon;
        }
    }

    public ItemSO CurrentTool { private set; get; }

    [Header("Hand Tools")]
    public Transform ItemHandParent; // parent object holdings hand tools to see them in hands
  
    private List<ItemHand> tools = new List<ItemHand>();


    public Action EventOnToolChanged;
    public Transform Player { get; private set; }
    private void Start()
    {
        // this enables middle button mouse click work correctly
        SampleCameraController cameractrl = GetComponentInChildren<SampleCameraController>();
        if (cameractrl != null)
        {
            cameractrl.enabled = false;
            cameractrl.enabled = true;

        }
        Player = ikHandler.transform;
        tools.AddRange(ItemHandParent.GetComponentsInChildren<ItemHand>(true));

        SecondaryImage.sprite = Secundary.icon;
        PrimaryImage.sprite = primary.icon;


        Equip(null);
        EquipInHand(CurrentTool);

    }

    private void Equip(ItemSO item)
    {
        if (CurrentTool == item) return; // already equipped

        // animations, sound effects, etc.
        CurrentTool = item;
        EquipInHand(CurrentTool);

        // UI switch: pokud je item null, pøepneme na "none"- middleborder, jinak na primary/secondary podle pøiøazení
        if (item == null)
            uiTransition?.SwitchToNone();
        else
            uiTransition?.SwitchTool(item == Primary);

        EventOnToolChanged?.Invoke();
    }

    public void OnScroll(InputAction.CallbackContext ctx) // switch tool
    {
        if (!ctx.performed) return;

        Vector2 scrollValue = ctx.ReadValue<Vector2>();

        if (scrollValue.y > 0)
            Equip(Primary);
        else if (scrollValue.y < 0)
            Equip(Secundary);
    }
    public void OnMouseMiddleClick(InputAction.CallbackContext ctx) // no one uses it
    {
        if (!ctx.performed) return;

        Equip(null);

    }


    private void EquipInHand(ItemSO item)
    {
        foreach (var tool in tools)
        {
            if (tool.itemScriptableObject == item)
            {
                tool.SetActive(true);
                ikHandler.SetTargets(tool.ikLeftTarget, tool.ikRightTarget);
            }
            else
                tool.SetActive(false);
        }
        if (item == null)
        {
            ikHandler.SetTargets(null,null);
        }
    }

   

    public  void UpgradeTool(ItemSO newTool)
    {
        if (newTool == null) return;

        if(newTool.itemType == ItemType.Chopp)
        {
            Primary = newTool;
            EquipInHand(Primary);
        }
        else if (newTool.itemType == ItemType.Mine)
        {
            Secundary = newTool;
            EquipInHand(Secundary);
        }
        else
        {
            Debug.LogError("Trying to upgrade tool with non-tool item: " + newTool.ItemName);
        }
    }

}