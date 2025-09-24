using Synty.AnimationBaseLocomotion.Samples;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerEquipment : MonoBehaviour
{
    public ItemSO axe;
    public ItemSO pickaxe;
    
    public ItemSO currentTool;
    // Další logika pro upgrady, výmìnu atd.

    private void Start()
    {
        SampleCameraController cameractrl = GetComponentInChildren<SampleCameraController>();
        if (cameractrl != null)
        {
            cameractrl.enabled = false;
            cameractrl.enabled = true;

        }

    }

    private void Equip(ItemSO item)
    {
        if (currentTool == item) return; // already equipped

        // animations, sound effects, etc.
        currentTool = item;
        Debug.Log($"Equipped {item.name}");
    }

    public void OnScroll(InputAction.CallbackContext ctx) // switch tool
    {
        if (!ctx.performed) return;

        Vector2 scrollValue = ctx.ReadValue<Vector2>();

        if (scrollValue.y > 0)
            Equip(axe);
        else if (scrollValue.y < 0)
            Equip(pickaxe);
    }
    public void OnMouseMiddleClick(InputAction.CallbackContext ctx) // no one uses it
    {
        if (!ctx.performed) return;

        Equip(null);

        Debug.Log("Støední tlaèítko myši kliknuto");
    }
}