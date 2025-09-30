using UnityEngine;
using static InteractManager;

public class TreeResource : Resource, IInteractable
{
    protected override void Start()
    {
        base.Start();

        miningAnimationName = AnimationType.Chopping;
    }

    protected override void LateStart()
    {
        base.LateStart();
        ItemSO = resourceMapManager.GetWoodSO();
    }

    protected override void Update()
    {
        base.Update();
    }

    public override void Interact(InteractManager interactor, InteractAction action)
    {
        base.Interact(interactor, action);

        
        switch (action)
        {
            case InteractAction.E:
                break;

            case InteractAction.HoldStart:
                StartMining();
                break;

            case InteractAction.Hold:
                break;

            case InteractAction.HoldEnd:
                StopMining();

                break;
        }
    }

    public override void OnHoverEnter() { base.OnHoverEnter(); }
    public override void OnHoverExit() { base.OnHoverExit(); }

    protected override void GetDrop(int ammount)
    {
        base.GetDrop(ammount);
        inventoryManager.AddResourceToHotbar(ItemSO, ammount);
    }
   
}
