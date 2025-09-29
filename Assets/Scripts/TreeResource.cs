using UnityEngine;
using static InteractManager;

public class TreeResource : Resource, IInteractable
{
    protected override void Start()
    {
        base.Start();

        miningAnimationName = AnimationType.Chopping;
    }

    public override void Interact(InteractManager interactor, InteractAction action)
    {
        base.Interact(interactor, action);


        switch (action)
        {
            case InteractAction.E:
                break;

            case InteractAction.HoldStart:
                isMining = true;
                toolAnimator?.SetBool("Is" + miningAnimationName, true);
                break;

            case InteractAction.Hold:
                break;

            case InteractAction.HoldEnd:
                isMining = false;
                toolAnimator?.SetBool("Is" + miningAnimationName, false);

                break;
        }
    }

    public override void OnHoverEnter() { base.OnHoverEnter(); }
    public override void OnHoverExit() { base.OnHoverExit(); }


    protected override void Update()
    {
        base.Update();
    }
}
