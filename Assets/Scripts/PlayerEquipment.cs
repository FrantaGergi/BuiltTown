using NUnit.Framework;
using Synty.AnimationBaseLocomotion.Samples;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private IKHandler ikHandler;

    [Header("Tools")]
    public ItemSO Primary;
    public ItemSO Secundary;
    
    public ItemSO CurrentTool;

    [Header("Hand Tools")]
    public Transform ItemHandParent; // parent object holdings hand tools to see them in hands
  
    private List<ItemHand> tools = new List<ItemHand>();

    private void Start()
    {
        // this enables middle button mouse click work correctly
        SampleCameraController cameractrl = GetComponentInChildren<SampleCameraController>();
        if (cameractrl != null)
        {
            cameractrl.enabled = false;
            cameractrl.enabled = true;

        }

        tools.AddRange(ItemHandParent.GetComponentsInChildren<ItemHand>(true));

    }

    private void Equip(ItemSO item)
    {
        if (CurrentTool == item) return; // already equipped

        // animations, sound effects, etc.
        CurrentTool = item;
        EquipInHand(CurrentTool);

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

        Debug.Log("Støední tlaèítko myši kliknuto");
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