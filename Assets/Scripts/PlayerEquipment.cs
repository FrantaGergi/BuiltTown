using Synty.AnimationBaseLocomotion.Samples;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private IKHandler ikHandler;
    [SerializeField] private ToolIconTransition uiTransition;

    [Header("Tools")]
    public ItemSO Primary;
    public ItemSO Secundary;
    
    public ItemSO CurrentTool;

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

}