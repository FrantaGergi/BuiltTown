using System;
using UnityEngine;
using static InteractManager;

public class Resource : MonoBehaviour, IInteractable
{
    protected bool isMining;

    protected float lastLoopCount = 0f;
    protected Animator toolAnimator;


    protected AnimationType miningAnimationName;
    [Range(10, 1000),SerializeField]
    public int hitPoints = 10;
    [Range(3, 100), SerializeField]
    public int hitsPerDrop = 3;

    protected int hitsTaken = 0;

    protected enum AnimationType
    {
        Chopping,
        Mining
    }

    protected virtual void Start()
    {
    
    }

    public virtual void Interact(InteractManager interactor, InteractAction action)
    {
        if (toolAnimator == null && interactor != null)
        {
            toolAnimator = interactor.GetToolAnimator();
        }

    }

    public virtual void OnHoverEnter() { }
    public virtual void OnHoverExit() { }


    protected virtual void Update()
    {
        if (isMining)
        {

            if (toolAnimator != null)
            {
                DoCheck(toolAnimator);
            }
        }
    }

    protected void DoCheck(Animator toolAnimator)
    {
        AnimatorStateInfo state = toolAnimator.GetCurrentAnimatorStateInfo(0);
        if (state.IsName(miningAnimationName.ToString()))
        {
            // získáme èíslo aktuální smyèky (0, 1, 2...)
            int loopCount = Mathf.FloorToInt(state.normalizedTime);
            // když se èíslo smyèky zmìnilo od minula
            if (loopCount > lastLoopCount)
            {
                Debug.Log($"Jedna {miningAnimationName} smyèka dokonèena!");
                // tady spustíš tìžbu (pøidat kámen, zahrát zvuk...)
            }

            lastLoopCount = loopCount;
        }
        else
        {
            // reset, když nejsme v Mining animaci
            lastLoopCount = 0;
        }
    }

    public virtual int TakeHit(int damagePoint)
    {
        hitPoints -= damagePoint;
        if (hitPoints <= 0)
        {
            DestroyResource();
        }

        hitsTaken += damagePoint;
        if(hitsTaken >= hitsPerDrop)
        {
            hitsTaken = 0;
            return 1; // drop item
        }
        return 0;
    }

    protected virtual void DestroyResource()
    {
        Destroy(gameObject);
    }

   

}
